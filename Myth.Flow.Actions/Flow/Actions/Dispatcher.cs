using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Myth.Flow.Interfaces;
using System.Diagnostics;

namespace Myth.Flow.Actions {

	/// <summary>
	/// Default dispatcher implementation
	/// </summary>
	internal sealed class Dispatcher : IDispatcher {
		private readonly IServiceProvider _serviceProvider;
		private readonly ICacheProvider? _cacheProvider;
		private readonly IEventBus _eventBus;
		private readonly ILogger<Dispatcher> _logger;
		private readonly ActivitySource _activitySource;

		public Dispatcher(
			IServiceProvider serviceProvider,
			IEventBus eventBus,
			ILogger<Dispatcher> logger,
			ActivitySource activitySource,
			ICacheProvider? cacheProvider = null ) {
			_serviceProvider = serviceProvider;
			_cacheProvider = cacheProvider;
			_eventBus = eventBus;
			_logger = logger;
			_activitySource = activitySource;
		}

		public async Task<CommandResult> DispatchCommandAsync<TCommand>(
			TCommand command,
			CancellationToken cancellationToken = default )
			where TCommand : ICommand {
			using var activity = _activitySource.StartActivity( $"Command.{typeof( TCommand ).Name}" );

			try {
				_logger.LogInformation( "Dispatching command {CommandType}", typeof( TCommand ).Name );

				var handler = _serviceProvider.GetService<ICommandHandler<TCommand>>( )
					?? throw new HandlerNotFoundException( $"No handler registered for command {typeof( TCommand ).Name}" );

				var result = await handler.HandleAsync( command, cancellationToken );

				activity?.SetStatus( result.IsSuccess ? ActivityStatusCode.Ok : ActivityStatusCode.Error );

				_logger.LogInformation( "Command {CommandType} executed with result: {IsSuccess}",
					typeof( TCommand ).Name, result.IsSuccess );

				return result;
			} catch ( Exception ex ) {
				activity?.SetStatus( ActivityStatusCode.Error, ex.Message );
				_logger.LogError( ex, "Error dispatching command {CommandType}", typeof( TCommand ).Name );
				return CommandResult.Failure( ex.Message, ex );
			}
		}

		public async Task<CommandResult<TResponse>> DispatchCommandAsync<TCommand, TResponse>(
			TCommand command,
			CancellationToken cancellationToken = default )
			where TCommand : ICommand<TResponse> {
			using var activity = _activitySource.StartActivity( $"Command.{typeof( TCommand ).Name}" );

			try {
				_logger.LogInformation( "Dispatching command {CommandType}", typeof( TCommand ).Name );

				var handler = _serviceProvider.GetService<ICommandHandler<TCommand, TResponse>>( )
					?? throw new HandlerNotFoundException( $"No handler registered for command {typeof( TCommand ).Name}" );

				var result = await handler.HandleAsync( command, cancellationToken );

				activity?.SetStatus( result.IsSuccess ? ActivityStatusCode.Ok : ActivityStatusCode.Error );

				_logger.LogInformation( "Command {CommandType} executed with result: {IsSuccess}",
					typeof( TCommand ).Name, result.IsSuccess );

				return result;
			} catch ( Exception ex ) {
				activity?.SetStatus( ActivityStatusCode.Error, ex.Message );
				_logger.LogError( ex, "Error dispatching command {CommandType}", typeof( TCommand ).Name );
				return CommandResult<TResponse>.Failure( ex.Message, ex );
			}
		}

		public async Task<QueryResult<TResponse>> DispatchQueryAsync<TQuery, TResponse>(
			TQuery query,
			CacheOptions? cacheOptions = null,
			CancellationToken cancellationToken = default )
			where TQuery : IQuery<TResponse> {
			using var activity = _activitySource.StartActivity( $"Query.{typeof( TQuery ).Name}" );

			try {
				var cacheKey = cacheOptions?.CacheKey ?? GenerateCacheKey( query );

				if ( cacheOptions?.Enabled == true && _cacheProvider != null ) {
					var cached = await _cacheProvider.GetAsync<TResponse>( cacheKey, cancellationToken );
					if ( cached.HasValue ) {
						_logger.LogInformation( "Query {QueryType} served from cache", typeof( TQuery ).Name );
						activity?.SetTag( "cache.hit", true );
						return QueryResult<TResponse>.Success( cached.Value, fromCache: true );
					}
				}

				_logger.LogInformation( "Dispatching query {QueryType}", typeof( TQuery ).Name );

				var handler = _serviceProvider.GetService<IQueryHandler<TQuery, TResponse>>( )
					?? throw new HandlerNotFoundException( $"No handler registered for query {typeof( TQuery ).Name}" );

				var result = await handler.HandleAsync( query, cancellationToken );

				if ( result.IsSuccess && cacheOptions?.Enabled == true && _cacheProvider != null ) {
					await _cacheProvider.SetAsync(
						cacheKey,
						result.Data!,
						cacheOptions.Ttl,
						cacheOptions.SlidingExpiration,
						cancellationToken );
				}

				activity?.SetStatus( result.IsSuccess ? ActivityStatusCode.Ok : ActivityStatusCode.Error );
				activity?.SetTag( "cache.hit", false );

				return result;
			} catch ( Exception ex ) {
				activity?.SetStatus( ActivityStatusCode.Error, ex.Message );
				_logger.LogError( ex, "Error dispatching query {QueryType}", typeof( TQuery ).Name );
				return QueryResult<TResponse>.Failure( ex.Message, ex );
			}
		}

		public async Task PublishEventAsync<TEvent>( TEvent @event, CancellationToken cancellationToken = default )
			where TEvent : IEvent {
			using var activity = _activitySource.StartActivity( $"Event.{typeof( TEvent ).Name}" );

			try {
				_logger.LogInformation( "Publishing event {EventType} with ID {EventId}",
					typeof( TEvent ).Name, @event.EventId );

				await _eventBus.PublishAsync( @event, cancellationToken );

				activity?.SetStatus( ActivityStatusCode.Ok );
			} catch ( Exception ex ) {
				activity?.SetStatus( ActivityStatusCode.Error, ex.Message );
				_logger.LogError( ex, "Error publishing event {EventType}", typeof( TEvent ).Name );
				throw;
			}
		}

		private static string GenerateCacheKey<TQuery>( TQuery query ) {
			var typeName = typeof( TQuery ).Name;
			var hashCode = query?.GetHashCode( ) ?? 0;
			return $"{typeName}:{hashCode}";
		}
	}
}