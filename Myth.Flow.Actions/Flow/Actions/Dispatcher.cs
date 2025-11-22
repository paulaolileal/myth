using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Myth.Interfaces;
using Myth.Models;
using Myth.ServiceProvider;

namespace Myth.Flow.Actions;

/// <summary>
/// Default dispatcher implementation that uses MythServiceProvider for dependency resolution
/// </summary>
internal sealed class Dispatcher(
	IEventBus eventBus,
	ILogger<Dispatcher> logger,
	ActivitySource activitySource,
	ICacheProvider? cacheProvider = null,
	Myth.Models.PipelineConfiguration? pipelineConfiguration = null ) : IDispatcher {
	private readonly ICacheProvider? _cacheProvider = cacheProvider;
	private readonly IEventBus _eventBus = eventBus;
	private readonly ILogger<Dispatcher> _logger = logger;
	private readonly ActivitySource _activitySource = activitySource;
	private readonly Myth.Models.PipelineConfiguration? _pipelineConfiguration = pipelineConfiguration;

	/// <summary>
	/// Dispatches a command to its registered handler
	/// </summary>
	/// <typeparam name="TCommand">The type of command to dispatch</typeparam>
	/// <param name="command">The command instance to execute</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	/// <returns>A task containing the command execution result</returns>
	public async Task<CommandResult> DispatchCommandAsync<TCommand>(
		TCommand command,
		CancellationToken cancellationToken = default )
		where TCommand : ICommand {
		using var activity = _activitySource.StartActivity( $"Command.{typeof( TCommand ).Name}" );

		try {
			_logger.LogInformation( "Dispatching command {CommandType}", typeof( TCommand ).Name );

			var handler = MythServiceProvider.GetRequired( ).GetService<ICommandHandler<TCommand>>( )
				?? throw new HandlerNotFoundException( $"No handler registered for command {typeof( TCommand ).Name}" );

			var result = await handler.HandleAsync( command, cancellationToken );

			activity?.SetStatus( result.IsSuccess ? ActivityStatusCode.Ok : ActivityStatusCode.Error );

			_logger.LogInformation( "Command {CommandType} executed with result: {IsSuccess}",
				typeof( TCommand ).Name, result.IsSuccess );

			return result;
		} catch ( Exception ex ) when ( ShouldPropagateException( ex ) ) {
			activity?.SetStatus( ActivityStatusCode.Error, ex.Message );
			_logger.LogError( ex, "Error dispatching command {CommandType} - exception propagated", typeof( TCommand ).Name );
			throw;
		} catch ( Exception ex ) {
			activity?.SetStatus( ActivityStatusCode.Error, ex.Message );
			_logger.LogError( ex, "Error dispatching command {CommandType}", typeof( TCommand ).Name );
			return CommandResult.Failure( ex.Message, ex );
		}
	}

	/// <summary>
	/// Dispatches a command to its registered handler and returns a typed response
	/// </summary>
	/// <typeparam name="TCommand">The type of command to dispatch</typeparam>
	/// <typeparam name="TResponse">The type of response expected from the command</typeparam>
	/// <param name="command">The command instance to execute</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	/// <returns>A task containing the command execution result with typed response</returns>
	public async Task<CommandResult<TResponse>> DispatchCommandAsync<TCommand, TResponse>(
		TCommand command,
		CancellationToken cancellationToken = default )
		where TCommand : ICommand<TResponse> {
		using var activity = _activitySource.StartActivity( $"Command.{typeof( TCommand ).Name}" );

		try {
			_logger.LogInformation( "Dispatching command {CommandType}", typeof( TCommand ).Name );

			var handler = MythServiceProvider.GetRequired( ).GetService<ICommandHandler<TCommand, TResponse>>( )
				?? throw new HandlerNotFoundException( $"No handler registered for command {typeof( TCommand ).Name}" );

			var result = await handler.HandleAsync( command, cancellationToken );

			activity?.SetStatus( result.IsSuccess ? ActivityStatusCode.Ok : ActivityStatusCode.Error );

			_logger.LogInformation( "Command {CommandType} executed with result: {IsSuccess}",
				typeof( TCommand ).Name, result.IsSuccess );

			return result;
		} catch ( Exception ex ) when ( ShouldPropagateException( ex ) ) {
			activity?.SetStatus( ActivityStatusCode.Error, ex.Message );
			_logger.LogError( ex, "Error dispatching command {CommandType} - exception propagated", typeof( TCommand ).Name );
			throw;
		} catch ( Exception ex ) {
			activity?.SetStatus( ActivityStatusCode.Error, ex.Message );
			_logger.LogError( ex, "Error dispatching command {CommandType}", typeof( TCommand ).Name );
			return CommandResult<TResponse>.Failure( ex.Message, ex );
		}
	}

	/// <summary>
	/// Dispatches a query to its registered handler with optional caching
	/// </summary>
	/// <typeparam name="TQuery">The type of query to dispatch</typeparam>
	/// <typeparam name="TResponse">The type of response expected from the query</typeparam>
	/// <param name="query">The query instance to execute</param>
	/// <param name="cacheOptions">Optional caching configuration for the query result</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	/// <returns>A task containing the query execution result with typed response</returns>
	public async Task<QueryResult<TResponse>> DispatchQueryAsync<TQuery, TResponse>(
		TQuery query,
		CacheOptions? cacheOptions = null,
		CancellationToken cancellationToken = default )
		where TQuery : IQuery<TResponse> {
		using var activity = _activitySource.StartActivity( $"Query.{typeof( TQuery ).Name}" );

		try {
			string? cacheKey = null;
			if ( cacheOptions?.Enabled == true ) {
				// Prioridade: KeyGenerator > CacheKey explícita > Auto-geração
				if ( cacheOptions.KeyGenerator != null ) {
					cacheKey = cacheOptions.KeyGenerator( query );
				} else if ( !string.IsNullOrEmpty( cacheOptions.CacheKey ) ) {
					cacheKey = cacheOptions.CacheKey;
				} else {
					cacheKey = GenerateCacheKey( query );
				}
			}

			if ( cacheOptions?.Enabled == true && _cacheProvider != null && !string.IsNullOrEmpty( cacheKey ) ) {
				var cached = await _cacheProvider.GetAsync<TResponse>( cacheKey, cancellationToken );
				if ( cached.HasValue ) {
					_logger.LogInformation( "Query {QueryType} served from cache", typeof( TQuery ).Name );
					activity?.SetTag( "cache.hit", true );
					return QueryResult<TResponse>.Success( cached.Value, fromCache: true );
				}
			}

			_logger.LogInformation( "Dispatching query {QueryType}", typeof( TQuery ).Name );

			var handler = MythServiceProvider.GetRequired( ).GetService<IQueryHandler<TQuery, TResponse>>( )
				?? throw new HandlerNotFoundException( $"No handler registered for query {typeof( TQuery ).Name}" );

			var result = await handler.HandleAsync( query, cancellationToken );

			if ( result.IsSuccess && cacheOptions?.Enabled == true && _cacheProvider != null && !string.IsNullOrEmpty( cacheKey ) ) {
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
		} catch ( Exception ex ) when ( ShouldPropagateException( ex ) ) {
			activity?.SetStatus( ActivityStatusCode.Error, ex.Message );
			_logger.LogError( ex, "Error dispatching query {QueryType} - exception propagated", typeof( TQuery ).Name );
			throw;
		} catch ( Exception ex ) {
			activity?.SetStatus( ActivityStatusCode.Error, ex.Message );
			_logger.LogError( ex, "Error dispatching query {QueryType}", typeof( TQuery ).Name );
			return QueryResult<TResponse>.Failure( ex.Message, ex );
		}
	}

	/// <summary>
	/// Publishes an event to the event bus for processing by registered handlers
	/// </summary>
	/// <typeparam name="TEvent">The type of event to publish</typeparam>
	/// <param name="event">The event instance to publish</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	/// <returns>A task representing the asynchronous publish operation</returns>
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

	/// <summary>
	/// Generates a cache key for a query using JSON serialization for parameter consistency
	/// </summary>
	/// <typeparam name="TQuery">The type of query</typeparam>
	/// <param name="query">The query instance</param>
	/// <returns>A cache key string in the format "TypeName:Hash" where Hash is derived from JSON serialization</returns>
	private static string GenerateCacheKey<TQuery>( TQuery query ) {
		var typeName = typeof( TQuery ).Name;

		try {
			// Serialização determinística para garantir consistência
			var jsonOptions = new JsonSerializerOptions {
				WriteIndented = false,
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				IgnoreNullValues = true,
				IncludeFields = false
			};

			var serialized = JsonSerializer.Serialize( query, jsonOptions );
			var hash = ComputeStableHash( serialized );

			return $"{typeName}:{hash}";
		} catch {
			// Fallback para GetHashCode() em caso de erro na serialização
			var hashCode = query?.GetHashCode( ) ?? 0;
			return $"{typeName}:{hashCode}";
		}
	}

	/// <summary>
	/// Computes a stable hash from a string using SHA256
	/// </summary>
	/// <param name="input">The input string to hash</param>
	/// <returns>A 16-character hexadecimal hash string</returns>
	private static string ComputeStableHash( string input ) {
		using var sha256 = SHA256.Create( );
		var bytes = Encoding.UTF8.GetBytes( input );
		var hashBytes = sha256.ComputeHash( bytes );
		return Convert.ToHexString( hashBytes )[ ..16 ]; // Primeiros 16 caracteres para performance
	}

	/// <summary>
	/// Determines whether an exception should be propagated based on the configured exception filter.
	/// </summary>
	/// <param name="exception">The exception to evaluate.</param>
	/// <returns>True if the exception should be propagated; otherwise, false.</returns>
	private bool ShouldPropagateException( Exception exception ) {
		if ( _pipelineConfiguration?.ExceptionTypesToPropagate.Count == 0 ) {
			return false;
		}

		var exceptionType = exception.GetType( );

		// Check if the exception type or any of its base types should be propagated
		return _pipelineConfiguration?.ExceptionTypesToPropagate.Any( configuredType =>
			configuredType.IsAssignableFrom( exceptionType ) ) ?? false;
	}
}
