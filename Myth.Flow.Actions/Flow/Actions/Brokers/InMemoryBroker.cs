using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Myth.Flow.Actions.Settings;
using Myth.Flow.Resilience;
using Myth.Interfaces;
using Myth.Models;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Channels;

namespace Myth.Flow.Actions.Brokers;

/// <summary>
/// Production-ready in-memory message broker with retry, DLQ, and parallel processing
/// </summary>
public sealed class InMemoryBroker : IMessageBroker, IAsyncDisposable {
	private readonly IServiceProvider _serviceProvider;
	private readonly IEventSubscriptionManager _subscriptionManager;
	private readonly ILogger<InMemoryBroker> _logger;
	private readonly ActivitySource _activitySource;
	private readonly InMemoryBrokerOptions _options;

	private readonly Channel<MessageEnvelope> _channel;
	private readonly ConcurrentDictionary<string, int> _retryCount = new( );
	private readonly DeadLetterQueue? _deadLetterQueue;
	private readonly SemaphoreSlim _concurrencySemaphore;

	private Task? _processingTask;
	private CancellationTokenSource? _cts;

	public InMemoryBroker(
		IServiceProvider serviceProvider,
		IEventSubscriptionManager subscriptionManager,
		ILogger<InMemoryBroker> logger,
		ActivitySource activitySource,
		InMemoryBrokerOptions? options = null,
		DeadLetterQueue? deadLetterQueue = null ) {
		_serviceProvider = serviceProvider;
		_subscriptionManager = subscriptionManager;
		_logger = logger;
		_activitySource = activitySource;
		_options = options ?? new InMemoryBrokerOptions( );
		_deadLetterQueue = deadLetterQueue;

		_channel = Channel.CreateBounded<MessageEnvelope>( new BoundedChannelOptions( _options.ChannelCapacity ) {
			FullMode = BoundedChannelFullMode.Wait
		} );

		_concurrencySemaphore = new SemaphoreSlim( _options.MaxConcurrentHandlers );
	}

	public async Task PublishAsync<TEvent>( TEvent @event, CancellationToken cancellationToken = default )
		where TEvent : IEvent {
		using var activity = _activitySource.StartActivity( "InMemoryBroker.Publish" );
		activity?.SetTag( "event.type", typeof( TEvent ).Name );
		activity?.SetTag( "event.id", @event.EventId );

		try {
			var envelope = CreateEnvelope( @event );
			await _channel.Writer.WriteAsync( envelope, cancellationToken );

			_logger.LogDebug(
				"Published event {EventType} with ID {EventId} to in-memory channel",
				typeof( TEvent ).Name,
				@event.EventId );

			activity?.SetStatus( ActivityStatusCode.Ok );
		} catch ( Exception ex ) {
			activity?.SetStatus( ActivityStatusCode.Error, ex.Message );
			_logger.LogError( ex, "Error publishing event {EventType}", typeof( TEvent ).Name );
			throw;
		}
	}

	public Task StartAsync( CancellationToken cancellationToken = default ) {
		_logger.LogInformation(
			"Starting in-memory message broker with {Workers} workers and {Capacity} channel capacity",
			_options.WorkerCount,
			_options.ChannelCapacity );

		_cts = new CancellationTokenSource( );

		// Start multiple worker tasks for parallel processing
		var workers = new List<Task>( );
		for ( int i = 0; i < _options.WorkerCount; i++ ) {
			var workerId = i;
			workers.Add( Task.Run( ( ) => ProcessMessagesAsync( workerId, _cts.Token ), _cts.Token ) );
		}

		_processingTask = Task.WhenAll( workers );

		return Task.CompletedTask;
	}

	public async Task StopAsync( CancellationToken cancellationToken = default ) {
		_logger.LogInformation( "Stopping in-memory message broker" );

		_channel.Writer.Complete( );
		_cts?.Cancel( );

		if ( _processingTask != null ) {
			try {
				await _processingTask;
			} catch ( OperationCanceledException ) {
				// Expected when cancelling
			}
		}

		_logger.LogInformation( "In-memory message broker stopped" );
	}

	private async Task ProcessMessagesAsync( int workerId, CancellationToken cancellationToken ) {
		_logger.LogDebug( "Worker {WorkerId} started", workerId );

		try {
			await foreach ( var envelope in _channel.Reader.ReadAllAsync( cancellationToken ) ) {
				await _concurrencySemaphore.WaitAsync( cancellationToken );

				try {
					await ProcessMessageAsync( envelope, cancellationToken );
				} finally {
					_concurrencySemaphore.Release( );
				}
			}
		} catch ( OperationCanceledException ) {
			_logger.LogDebug( "Worker {WorkerId} cancelled", workerId );
		} catch ( Exception ex ) {
			_logger.LogError( ex, "Worker {WorkerId} encountered fatal error", workerId );
		}

		_logger.LogDebug( "Worker {WorkerId} stopped", workerId );
	}

	private async Task ProcessMessageAsync( MessageEnvelope envelope, CancellationToken cancellationToken ) {
		using var activity = _activitySource.StartActivity( "InMemoryBroker.Process" );
		activity?.SetTag( "message.id", envelope.MessageId );
		activity?.SetTag( "message.type", envelope.MessageType );

		var eventTypeName = envelope.MessageType;
		var attempts = _retryCount.GetOrAdd( envelope.MessageId, 0 );

		try {
			_logger.LogDebug(
				"Processing message {MessageId} of type {MessageType} (attempt {Attempt})",
				envelope.MessageId,
				eventTypeName,
				attempts + 1 );

			// Deserialize event
			var eventType = Type.GetType( envelope.Headers[ "event.type.fullname" ] );
			if ( eventType == null ) {
				_logger.LogError( "Cannot resolve event type: {EventType}", envelope.MessageType );
				return;
			}

			var @event = JsonSerializer.Deserialize( envelope.Payload, eventType );
			if ( @event == null ) {
				_logger.LogError( "Failed to deserialize event: {MessageId}", envelope.MessageId );
				return;
			}

			// Get handlers for this event type
			var handlers = _subscriptionManager.GetHandlersForEvent( eventType );

			if ( !handlers.Any( ) ) {
				_logger.LogWarning(
					"No handlers registered for event type {EventType}",
					eventTypeName );
				return;
			}

			// Invoke all handlers in parallel
			var tasks = new List<Task>( );
			foreach ( var handlerType in handlers ) {
				tasks.Add( InvokeHandlerAsync( @event, eventType, handlerType, envelope, cancellationToken ) );
			}

			await Task.WhenAll( tasks );

			// Remove from retry tracking on success
			_retryCount.TryRemove( envelope.MessageId, out _ );

			activity?.SetStatus( ActivityStatusCode.Ok );

			_logger.LogDebug(
				"Successfully processed message {MessageId} with {HandlerCount} handlers",
				envelope.MessageId,
				handlers.Count( ) );
		} catch ( Exception ex ) {
			activity?.SetStatus( ActivityStatusCode.Error, ex.Message );

			_logger.LogError(
				ex,
				"Error processing message {MessageId} of type {MessageType} (attempt {Attempt})",
				envelope.MessageId,
				eventTypeName,
				attempts + 1 );

			await HandleFailureAsync( envelope, ex, attempts, cancellationToken );
		}
	}

	private async Task InvokeHandlerAsync(
	object @event,
	Type eventType,
	Type handlerType,
	MessageEnvelope envelope,
	CancellationToken cancellationToken ) {
		using var activity = _activitySource.StartActivity( $"Handler.{handlerType.Name}" );
		using var scope = _serviceProvider.CreateScope( );

		try {
			var eventHandlerInterface = typeof( IEventHandler<> ).MakeGenericType( eventType );

			var handler = scope.ServiceProvider.GetService( eventHandlerInterface );

			if ( handler == null ) {
				_logger.LogWarning(
					"Handler for event type {EventType} not resolved from DI",
					eventType.Name );
				return;
			}

			// Método HandleAsync está na interface
			var handleMethod = eventHandlerInterface.GetMethod( "HandleAsync",
				[ eventType, typeof( CancellationToken ) ] );

			if ( handleMethod == null ) {
				_logger.LogError( "HandleAsync method not found" );
				return;
			}

			// Invocar
			var task = handleMethod.Invoke( handler, new[ ] { @event, cancellationToken } ) as Task;
			if ( task != null ) {
				await task;
			}

			activity?.SetStatus( ActivityStatusCode.Ok );
		} catch ( Exception ex ) {
			activity?.SetStatus( ActivityStatusCode.Error, ex.Message );
			_logger.LogError( ex, "Handler {HandlerType} failed", handlerType.Name );
			throw;
		}
	}

	private async Task HandleFailureAsync(
		MessageEnvelope envelope,
		Exception exception,
		int attempts,
		CancellationToken cancellationToken ) {
		if ( attempts < _options.MaxRetryAttempts ) {
			// Increment retry count
			_retryCount.AddOrUpdate( envelope.MessageId, 1, ( _, count ) => count + 1 );

			// Calculate backoff delay
			var delay = CalculateBackoffDelay( attempts );

			_logger.LogWarning(
				"Retrying message {MessageId} after {Delay}ms (attempt {Attempt}/{MaxAttempts})",
				envelope.MessageId,
				delay.TotalMilliseconds,
				attempts + 1,
				_options.MaxRetryAttempts );

			// Wait and retry
			await Task.Delay( delay, cancellationToken );
			await _channel.Writer.WriteAsync( envelope, cancellationToken );
		} else {
			_logger.LogError(
				exception,
				"Message {MessageId} failed after {MaxAttempts} attempts, moving to DLQ",
				envelope.MessageId,
				_options.MaxRetryAttempts );

			// Remove from retry tracking
			_retryCount.TryRemove( envelope.MessageId, out _ );

			// Send to DLQ
			if ( _deadLetterQueue != null ) {
				var metadata = new EventMetadata {
					EventId = envelope.MessageId,
					EventType = envelope.MessageType,
					OccurredAt = envelope.Timestamp,
					ProcessedAt = DateTimeOffset.UtcNow,
					RetryCount = attempts
				};

				_deadLetterQueue.Enqueue( envelope, exception, metadata );
			}
		}
	}

	private TimeSpan CalculateBackoffDelay( int attempts ) {
		if ( !_options.ExponentialBackoff )
			return TimeSpan.FromMilliseconds( _options.RetryBackoffMs );

		var exponentialDelay = _options.RetryBackoffMs * Math.Pow( 2, attempts );
		return TimeSpan.FromMilliseconds( Math.Min( exponentialDelay, _options.MaxBackoffMs ) );
	}

	private MessageEnvelope CreateEnvelope<TEvent>( TEvent @event ) where TEvent : IEvent {
		var payload = JsonSerializer.Serialize( @event, @event.GetType( ) );

		return new MessageEnvelope {
			MessageId = @event.EventId,
			MessageType = typeof( TEvent ).Name,
			Payload = payload,
			Timestamp = DateTimeOffset.UtcNow,
			CorrelationId = Activity.Current?.Id,
			Headers = new Dictionary<string, string> {
				[ "event.type.fullname" ] = typeof( TEvent ).AssemblyQualifiedName ?? typeof( TEvent ).FullName ?? typeof( TEvent ).Name,
				[ "event.occurred_at" ] = @event.OccurredAt.ToString( "O" )
			}
		};
	}

	public async ValueTask DisposeAsync( ) {
		await StopAsync( );
		_concurrencySemaphore?.Dispose( );
		_cts?.Dispose( );
	}
}