using Microsoft.Extensions.DependencyInjection;
using Myth.Exceptions;
using Myth.Flow.Actions.Interfaces;
using Myth.Interfaces;
using Myth.Models;

namespace Myth.Flow.Actions.Extensions;

/// <summary>
/// Extension methods for Action-First pipeline pattern without context boilerplate
/// </summary>
public static class PipelineExtensions {

	/// <summary>
	/// Starts a new action pipeline with an initial request
	/// </summary>
	/// <typeparam name="TRequest">The initial request type</typeparam>
	/// <param name="request">The initial request to start the pipeline with</param>
	/// <param name="serviceProvider">Service provider for dependency injection</param>
	/// <returns>Action pipeline builder for method chaining</returns>
	public static IActionPipelineBuilder<TRequest> Start<TRequest>( TRequest request, IServiceProvider serviceProvider ) {
		var state = new ActionPipelineState<TRequest>( request, serviceProvider );
		var pipeline = Myth.Flow.Pipeline.Start( state, serviceProvider );
		return new ActionPipelineBuilder<TRequest>( pipeline );
	}

	/// <summary>
	/// Starts a new action pipeline with an initial request using default service provider
	/// </summary>
	/// <typeparam name="TRequest">The initial request type</typeparam>
	/// <param name="request">The initial request to start the pipeline with</param>
	/// <returns>Action pipeline builder for method chaining</returns>
	public static IActionPipelineBuilder<TRequest> Start<TRequest>( TRequest request ) {
		var serviceProvider = ( IServiceProvider )new ServiceCollection( ).BuildServiceProvider( );
		return Start( request, serviceProvider );
	}

	/// <summary>
	/// Starts an empty action pipeline for functional/utility scenarios
	/// </summary>
	/// <param name="serviceProvider">Service provider for dependency injection</param>
	/// <returns>Empty pipeline builder that can be populated with Transform operations</returns>
	public static IEmptyPipelineBuilder Start( IServiceProvider serviceProvider ) {
		return new EmptyPipelineBuilder( serviceProvider );
	}

	/// <summary>
	/// Starts an empty action pipeline with default service provider
	/// </summary>
	/// <returns>Empty pipeline builder that can be populated with Transform operations</returns>
	public static IEmptyPipelineBuilder Start( ) {
		var serviceProvider = ( IServiceProvider )new ServiceCollection( ).BuildServiceProvider( );
		return Start( serviceProvider );
	}

	/// <summary>
	/// Processes a command that is already in the pipeline context
	/// </summary>
	/// <typeparam name="TCommand">The command type to execute</typeparam>
	/// <param name="builder">The action pipeline builder</param>
	/// <returns>The pipeline builder for method chaining</returns>
	/// <exception cref="PipelineException">Thrown when command execution fails</exception>
	public static IActionPipelineBuilder<TCommand> Process<TCommand>(
		this IActionPipelineBuilder<TCommand> builder )
		where TCommand : ICommand {
		var internalBuilder = ( ActionPipelineBuilder<TCommand> )builder;
		var newPipeline = internalBuilder.InnerPipeline.StepAsync<IDispatcher>( async ( dispatcher, state ) => {
			var command = state.CurrentRequest!;
			var result = await dispatcher.DispatchCommandAsync( command );

			if ( result.IsFailure )
				throw new PipelineException(
					result.ErrorMessage ?? "Command processing failed",
					result.Exception );

			state.LastResult = result;
			return state;
		} );

		return new ActionPipelineBuilder<TCommand>( newPipeline );
	}

	/// <summary>
	/// Processes a command with typed response that is already in the pipeline context
	/// </summary>
	/// <typeparam name="TCommand">The command type to execute</typeparam>
	/// <typeparam name="TResponse">The response type from the command</typeparam>
	/// <param name="builder">The action pipeline builder</param>
	/// <returns>The pipeline builder transformed to the response type</returns>
	/// <exception cref="PipelineException">Thrown when command execution fails</exception>
	public static IActionPipelineBuilder<TResponse> Process<TCommand, TResponse>(
		this IActionPipelineBuilder<TCommand> builder )
		where TCommand : ICommand<TResponse> {
		var internalBuilder = ( ActionPipelineBuilder<TCommand> )builder;
		var newPipeline = internalBuilder.InnerPipeline
			.StepAsync<IDispatcher>( async ( dispatcher, state ) => {
				var command = state.CurrentRequest!;
				var result = await dispatcher.DispatchCommandAsync<TCommand, TResponse>( command );

				if ( result.IsFailure )
					throw new PipelineException(
						result.ErrorMessage ?? "Command processing failed",
						result.Exception );

				state.LastResult = result;
				return state;
			} )
			.Transform( state => {
				var cmdResult = ( CommandResult<TResponse> )state.LastResult!;
				return new ActionPipelineState<TResponse>( cmdResult.Data!, state.ServiceProvider ) {
					LastResult = state.LastResult,
					CorrelationId = state.CorrelationId
				};
			} );

		return new ActionPipelineBuilder<TResponse>( newPipeline );
	}

	/// <summary>
	/// Executes a query that is already in the pipeline context without caching
	/// </summary>
	/// <typeparam name="TQuery">The query type to execute</typeparam>
	/// <typeparam name="TResponse">The response type from the query</typeparam>
	/// <param name="builder">The action pipeline builder</param>
	/// <returns>The pipeline builder transformed to the response type</returns>
	/// <exception cref="PipelineException">Thrown when query execution fails</exception>
	public static IActionPipelineBuilder<TResponse> Query<TQuery, TResponse>(
		this IActionPipelineBuilder<TQuery> builder )
		where TQuery : IQuery<TResponse> {
		return builder.Query<TQuery, TResponse>( null );
	}

	/// <summary>
	/// Executes a query that is already in the pipeline context with optional caching
	/// </summary>
	/// <typeparam name="TQuery">The query type to execute</typeparam>
	/// <typeparam name="TResponse">The response type from the query</typeparam>
	/// <param name="builder">The action pipeline builder</param>
	/// <param name="configureCache">Optional function to configure caching with access to query instance. If null, no caching is used</param>
	/// <returns>The pipeline builder transformed to the response type</returns>
	/// <exception cref="PipelineException">Thrown when query execution fails</exception>
	public static IActionPipelineBuilder<TResponse> Query<TQuery, TResponse>(
		this IActionPipelineBuilder<TQuery> builder,
		Func<TQuery, ICacheConfig, ICacheConfig>? configureCache )
		where TQuery : IQuery<TResponse> {
		var internalBuilder = ( ActionPipelineBuilder<TQuery> )builder;
		var newPipeline = internalBuilder.InnerPipeline
			.StepAsync<IDispatcher>( async ( dispatcher, state ) => {
				var query = state.CurrentRequest!;

				var cacheOptions = configureCache != null
					? ( ( CacheConfigBuilder )configureCache( query, new CacheConfigBuilder( ) ) ).ToCacheOptions( )
					: null;

				var result = await dispatcher.DispatchQueryAsync<TQuery, TResponse>( query, cacheOptions );

				if ( result.IsFailure )
					throw new PipelineException(
						result.ErrorMessage ?? "Query execution failed",
						result.Exception );

				state.LastResult = result;
				return state;
			} )
			.Transform( state => {
				var queryResult = ( QueryResult<TResponse> )state.LastResult!;
				return new ActionPipelineState<TResponse>( queryResult.Data!, state.ServiceProvider ) {
					LastResult = state.LastResult,
					CorrelationId = state.CorrelationId
				};
			} );

		return new ActionPipelineBuilder<TResponse>( newPipeline );
	}

	/// <summary>
	/// Publishes an event that is already in the pipeline context
	/// </summary>
	/// <typeparam name="TEvent">The event type to publish</typeparam>
	/// <param name="builder">The action pipeline builder</param>
	/// <returns>The pipeline builder for method chaining</returns>
	public static IActionPipelineBuilder<TEvent> Publish<TEvent>(
		this IActionPipelineBuilder<TEvent> builder )
		where TEvent : IEvent {
		var internalBuilder = ( ActionPipelineBuilder<TEvent> )builder;
		var newPipeline = internalBuilder.InnerPipeline.StepAsync<IDispatcher>( async ( dispatcher, state ) => {
			var @event = state.CurrentRequest!;
			await dispatcher.PublishEventAsync( @event );

			state.LastResult = "Event published successfully";
			return state;
		} );

		return new ActionPipelineBuilder<TEvent>( newPipeline );
	}

	/// <summary>
	/// Transforms the current request to a new request type
	/// </summary>
	/// <typeparam name="TCurrent">The current request type</typeparam>
	/// <typeparam name="TNext">The new request type</typeparam>
	/// <param name="builder">The action pipeline builder</param>
	/// <param name="transform">Function to transform current request to new request</param>
	/// <returns>The pipeline builder with the new request type</returns>
	public static IActionPipelineBuilder<TNext> Transform<TCurrent, TNext>(
		this IActionPipelineBuilder<TCurrent> builder,
		Func<TCurrent, TNext> transform ) {
		var internalBuilder = ( ActionPipelineBuilder<TCurrent> )builder;
		var newPipeline = internalBuilder.InnerPipeline.Transform( state => {
			var current = state.CurrentRequest!;
			var next = transform( current );

			return new ActionPipelineState<TNext>( next, state.ServiceProvider ) {
				LastResult = state.LastResult,
				CorrelationId = state.CorrelationId
			};
		} );

		return new ActionPipelineBuilder<TNext>( newPipeline );
	}

	/// <summary>
	/// Transforms the current request to a new request type asynchronously
	/// </summary>
	/// <typeparam name="TCurrent">The current request type</typeparam>
	/// <typeparam name="TNext">The new request type</typeparam>
	/// <param name="builder">The action pipeline builder</param>
	/// <param name="transform">Async function to transform current request to new request</param>
	/// <returns>The pipeline builder with the new request type</returns>
	public static IActionPipelineBuilder<TNext> TransformAsync<TCurrent, TNext>(
		this IActionPipelineBuilder<TCurrent> builder,
		Func<TCurrent, Task<TNext>> transform ) {
		var internalBuilder = ( ActionPipelineBuilder<TCurrent> )builder;
		var newPipeline = internalBuilder.InnerPipeline.TransformAsync( async state => {
			var current = state.CurrentRequest!;
			var next = await transform( current );

			return new ActionPipelineState<TNext>( next, state.ServiceProvider ) {
				LastResult = state.LastResult,
				CorrelationId = state.CorrelationId
			};
		} );

		return new ActionPipelineBuilder<TNext>( newPipeline );
	}

	/// <summary>
	/// Conditionally transforms the current request to a new request type
	/// </summary>
	/// <typeparam name="TCurrent">The current request type</typeparam>
	/// <typeparam name="TNext">The new request type</typeparam>
	/// <param name="builder">The action pipeline builder</param>
	/// <param name="condition">Condition to evaluate</param>
	/// <param name="transform">Function to transform current request when condition is true</param>
	/// <returns>The pipeline builder with the new request type or null if condition is false</returns>
	public static IActionPipelineBuilder<TNext?> TransformIf<TCurrent, TNext>(
		this IActionPipelineBuilder<TCurrent> builder,
		Func<TCurrent, bool> condition,
		Func<TCurrent, TNext> transform ) {
		var internalBuilder = ( ActionPipelineBuilder<TCurrent> )builder;
		var newPipeline = internalBuilder.InnerPipeline.Transform( state => {
			var current = state.CurrentRequest!;
			var shouldTransform = condition( current );

			TNext? next = shouldTransform ? transform( current ) : default;

			return new ActionPipelineState<TNext?>( next, state.ServiceProvider ) {
				LastResult = state.LastResult,
				CorrelationId = state.CorrelationId
			};
		} );

		return new ActionPipelineBuilder<TNext?>( newPipeline );
	}

	/// <summary>
	/// Conditionally transforms the current request with different transforms for true/false conditions
	/// </summary>
	/// <typeparam name="TCurrent">The current request type</typeparam>
	/// <typeparam name="TNext">The new request type</typeparam>
	/// <param name="builder">The action pipeline builder</param>
	/// <param name="condition">Condition to evaluate</param>
	/// <param name="transformTrue">Function to transform when condition is true</param>
	/// <param name="transformFalse">Function to transform when condition is false</param>
	/// <returns>The pipeline builder with the new request type</returns>
	public static IActionPipelineBuilder<TNext> TransformIf<TCurrent, TNext>(
		this IActionPipelineBuilder<TCurrent> builder,
		Func<TCurrent, bool> condition,
		Func<TCurrent, TNext> transformTrue,
		Func<TCurrent, TNext> transformFalse ) {
		var internalBuilder = ( ActionPipelineBuilder<TCurrent> )builder;
		var newPipeline = internalBuilder.InnerPipeline.Transform( state => {
			var current = state.CurrentRequest!;
			var shouldUseTrue = condition( current );

			var next = shouldUseTrue ? transformTrue( current ) : transformFalse( current );

			return new ActionPipelineState<TNext>( next, state.ServiceProvider ) {
				LastResult = state.LastResult,
				CorrelationId = state.CorrelationId
			};
		} );

		return new ActionPipelineBuilder<TNext>( newPipeline );
	}
}

/// <summary>
/// Interface for empty pipeline builder that can be populated with Transform operations
/// </summary>
public interface IEmptyPipelineBuilder {

	/// <summary>
	/// Gets the service provider for dependency injection
	/// </summary>
	IServiceProvider ServiceProvider { get; }

	/// <summary>
	/// Transforms empty pipeline to a request type using a factory function
	/// </summary>
	/// <typeparam name="TRequest">The request type to create</typeparam>
	/// <param name="factory">Factory function to create the initial request</param>
	/// <returns>Action pipeline builder with the created request</returns>
	IActionPipelineBuilder<TRequest> Transform<TRequest>( Func<TRequest> factory );

	/// <summary>
	/// Transforms empty pipeline to a request type using an async factory function
	/// </summary>
	/// <typeparam name="TRequest">The request type to create</typeparam>
	/// <param name="factory">Async factory function to create the initial request</param>
	/// <returns>Action pipeline builder with the created request</returns>
	IActionPipelineBuilder<TRequest> TransformAsync<TRequest>( Func<Task<TRequest>> factory );
}

/// <summary>
/// Internal implementation of empty pipeline builder
/// </summary>
internal class EmptyPipelineBuilder : IEmptyPipelineBuilder {

	/// <summary>
	/// Gets the service provider for dependency injection
	/// </summary>
	public IServiceProvider ServiceProvider { get; }

	/// <summary>
	/// Initializes a new instance of EmptyPipelineBuilder
	/// </summary>
	/// <param name="serviceProvider">Service provider for dependency injection</param>
	public EmptyPipelineBuilder( IServiceProvider serviceProvider ) {
		ServiceProvider = serviceProvider;
	}

	/// <summary>
	/// Transforms empty pipeline to a request type using a factory function
	/// </summary>
	/// <typeparam name="TRequest">The request type to create</typeparam>
	/// <param name="factory">Factory function to create the initial request</param>
	/// <returns>Action pipeline builder with the created request</returns>
	public IActionPipelineBuilder<TRequest> Transform<TRequest>( Func<TRequest> factory ) {
		var request = factory( );
		var state = new ActionPipelineState<TRequest>( request, ServiceProvider );
		var pipeline = Myth.Flow.Pipeline.Start( state, ServiceProvider );
		return new ActionPipelineBuilder<TRequest>( pipeline );
	}

	/// <summary>
	/// Transforms empty pipeline to a request type using an async factory function
	/// </summary>
	/// <typeparam name="TRequest">The request type to create</typeparam>
	/// <param name="factory">Async factory function to create the initial request</param>
	/// <returns>Action pipeline builder with the created request</returns>
	public IActionPipelineBuilder<TRequest> TransformAsync<TRequest>( Func<Task<TRequest>> factory ) {
		var state = new ActionPipelineState<TRequest>( default!, ServiceProvider );
		var pipeline = Myth.Flow.Pipeline.Start( state, ServiceProvider )
			.StepAsync<IServiceProvider>( async ( _, s ) => {
				var request = await factory( );
				s.CurrentRequest = request;
				return s;
			} );
		return new ActionPipelineBuilder<TRequest>( pipeline );
	}
}