using Myth.Exceptions;
using Myth.Interfaces;
using Myth.Models;

namespace Myth.Flow.Actions.Extensions;

/// <summary>
/// Extension methods for integrating Flow.Actions with Myth.Flow pipelines
/// </summary>
public static class PipelineExtensions {

	/// <summary>
	/// Processes a command in the pipeline
	/// </summary>
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
	/// Processes a command with response in the pipeline
	/// </summary>
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
	/// Executes a query in the pipeline
	/// </summary>
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
	/// Executes a query with cache configuration in the pipeline
	/// </summary>
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
	/// Publishes an event in the pipeline
	/// </summary>
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
	/// Publishes an event from context property in the pipeline
	/// </summary>
	public static IPipelineBuilder<TContext> Publish<TContext>(
		this IPipelineBuilder<TContext> builder )
		where TContext : IEvent {
		return builder.StepAsync<IDispatcher>( async ( dispatcher, context ) => {
			await dispatcher.PublishEventAsync( context );

			return context;
		} );
	}
}