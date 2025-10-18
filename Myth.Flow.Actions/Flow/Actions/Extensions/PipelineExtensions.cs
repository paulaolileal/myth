using Myth.Exceptions;
using Myth.Interfaces;
using Myth.Models;

namespace Myth.Flow.Actions.Extensions;

/// <summary>
/// Extension methods for integrating Flow.Actions with Myth.Flow pipelines
/// </summary>
public static class PipelineExtensions {

	/// <summary>
	/// Processes a command in the pipeline using the dispatcher
	/// </summary>
	/// <typeparam name="TContext">The pipeline context type</typeparam>
	/// <typeparam name="TCommand">The command type to execute</typeparam>
	/// <param name="builder">The pipeline builder</param>
	/// <param name="commandFactory">Factory function to create the command from context</param>
	/// <returns>The pipeline builder for method chaining</returns>
	/// <exception cref="PipelineException">Thrown when command execution fails</exception>
	public static IPipelineBuilder<TContext> Process<TContext, TCommand>(
		this IPipelineBuilder<TContext> builder,
		Func<TContext, TCommand> commandFactory )
		where TCommand : ICommand {
		return builder.StepAsync<IDispatcher>( async ( dispatcher, context ) => {
			var command = commandFactory( context );
			var result = await dispatcher.DispatchCommandAsync( command );

			if ( result.IsFailure )
				throw new PipelineException(
					result.ErrorMessage ?? "Command processing failed",
					result.Exception );

			return context;
		} );
	}

	/// <summary>
	/// Processes a command with typed response in the pipeline
	/// </summary>
	/// <typeparam name="TContext">The pipeline context type</typeparam>
	/// <typeparam name="TCommand">The command type to execute</typeparam>
	/// <typeparam name="TResponse">The response type from the command</typeparam>
	/// <param name="builder">The pipeline builder</param>
	/// <param name="commandFactory">Factory function to create the command from context</param>
	/// <param name="onSuccess">Callback invoked with context and response data on successful execution</param>
	/// <returns>The pipeline builder for method chaining</returns>
	/// <exception cref="PipelineException">Thrown when command execution fails</exception>
	public static IPipelineBuilder<TContext> Process<TContext, TCommand, TResponse>(
		this IPipelineBuilder<TContext> builder,
		Func<TContext, TCommand> commandFactory,
		Action<TContext, TResponse> onSuccess )
		where TCommand : ICommand<TResponse> {
		return builder.StepAsync<IDispatcher>( async ( dispatcher, context ) => {
			var command = commandFactory( context );
			var result = await dispatcher.DispatchCommandAsync<TCommand, TResponse>( command );

			if ( result.IsFailure )
				throw new PipelineException(
					result.ErrorMessage ?? "Command processing failed",
					result.Exception );

			if ( result.Data != null )
				onSuccess( context, result.Data );

			return context;
		} );
	}

	/// <summary>
	/// Executes a query in the pipeline with optional caching support
	/// </summary>
	/// <typeparam name="TContext">The pipeline context type</typeparam>
	/// <typeparam name="TQuery">The query type to execute</typeparam>
	/// <typeparam name="TResponse">The response type from the query</typeparam>
	/// <param name="builder">The pipeline builder</param>
	/// <param name="queryFactory">Factory function to create the query from context</param>
	/// <param name="onSuccess">Callback invoked with context and response data on successful execution</param>
	/// <param name="configureCache">Optional action to configure caching for the query</param>
	/// <returns>The pipeline builder for method chaining</returns>
	/// <exception cref="PipelineException">Thrown when query execution fails</exception>
	public static IPipelineBuilder<TContext> Query<TContext, TQuery, TResponse>(
		this IPipelineBuilder<TContext> builder,
		Func<TContext, TQuery> queryFactory,
		Action<TContext, TResponse> onSuccess,
		Action<CacheOptions>? configureCache = null )
		where TQuery : IQuery<TResponse> {
		return builder.StepAsync<IDispatcher>( async ( dispatcher, context ) => {
			var query = queryFactory( context );

			var cacheOptions = new CacheOptions( );
			configureCache?.Invoke( cacheOptions );

			var result = await dispatcher.DispatchQueryAsync<TQuery, TResponse>( query, cacheOptions );

			if ( result.IsFailure )
				throw new PipelineException(
					result.ErrorMessage ?? "Query execution failed",
					result.Exception );

			if ( result.Data != null )
				onSuccess( context, result.Data );

			return context;
		} );
	}

	/// <summary>
	/// Executes a query with explicit cache configuration in the pipeline
	/// </summary>
	/// <typeparam name="TContext">The pipeline context type</typeparam>
	/// <typeparam name="TQuery">The query type to execute</typeparam>
	/// <typeparam name="TResponse">The response type from the query</typeparam>
	/// <param name="builder">The pipeline builder</param>
	/// <param name="queryFactory">Factory function to create the query from context</param>
	/// <param name="onSuccess">Callback invoked with context and response data on successful execution</param>
	/// <param name="cacheKey">The cache key to use for storing/retrieving the result</param>
	/// <param name="ttl">Time-to-live for the cached result. Default is 5 minutes</param>
	/// <param name="slidingExpiration">Whether to use sliding expiration (resets TTL on access)</param>
	/// <returns>The pipeline builder for method chaining</returns>
	/// <exception cref="PipelineException">Thrown when query execution fails</exception>
	public static IPipelineBuilder<TContext> Query<TContext, TQuery, TResponse>(
		this IPipelineBuilder<TContext> builder,
		Func<TContext, TQuery> queryFactory,
		Action<TContext, TResponse> onSuccess,
		string cacheKey,
		TimeSpan? ttl = null,
		bool slidingExpiration = false )
		where TQuery : IQuery<TResponse> {
		return builder.Query<TContext, TQuery, TResponse>(
			queryFactory,
			onSuccess,
			options => {
				options.Enabled = true;
				options.CacheKey = cacheKey;
				options.Ttl = ttl ?? TimeSpan.FromMinutes( 5 );
				options.SlidingExpiration = slidingExpiration;
			} );
	}

	/// <summary>
	/// Publishes an event in the pipeline using the event bus
	/// </summary>
	/// <typeparam name="TContext">The pipeline context type</typeparam>
	/// <typeparam name="TEvent">The event type to publish</typeparam>
	/// <param name="builder">The pipeline builder</param>
	/// <param name="eventFactory">Factory function to create the event from context</param>
	/// <returns>The pipeline builder for method chaining</returns>
	public static IPipelineBuilder<TContext> Publish<TContext, TEvent>(
		this IPipelineBuilder<TContext> builder,
		Func<TContext, TEvent> eventFactory )
		where TEvent : IEvent {
		return builder.StepAsync<IDispatcher>( async ( dispatcher, context ) => {
			var @event = eventFactory( context );
			await dispatcher.PublishEventAsync( @event );

			return context;
		} );
	}

	/// <summary>
	/// Publishes the context itself as an event when the context implements IEvent
	/// </summary>
	/// <typeparam name="TContext">The pipeline context type that implements IEvent</typeparam>
	/// <param name="builder">The pipeline builder</param>
	/// <returns>The pipeline builder for method chaining</returns>
	public static IPipelineBuilder<TContext> Publish<TContext>(
		this IPipelineBuilder<TContext> builder )
		where TContext : IEvent {
		return builder.StepAsync<IDispatcher>( async ( dispatcher, context ) => {
			await dispatcher.PublishEventAsync( context );

			return context;
		} );
	}
}