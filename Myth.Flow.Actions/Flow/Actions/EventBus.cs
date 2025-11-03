using Microsoft.Extensions.Logging;
using Myth.Interfaces;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Myth.Flow.Actions;

/// <summary>
/// Default event bus implementation that uses MythServiceProvider for dependency resolution
/// </summary>
internal sealed class EventBus : IEventBus {
	private readonly IMessageBroker _messageBroker;
	private readonly ILogger<EventBus> _logger;
	private readonly ActivitySource _activitySource;
	private readonly ConcurrentDictionary<Type, List<Type>> _subscriptions = new( );

	public EventBus(
		IMessageBroker messageBroker,
		ILogger<EventBus> logger,
		ActivitySource activitySource ) {
		_messageBroker = messageBroker;
		_logger = logger;
		_activitySource = activitySource;
	}

	/// <summary>
	/// Publishes an event to the message broker and invokes all registered local handlers
	/// </summary>
	/// <typeparam name="TEvent">The type of event to publish</typeparam>
	/// <param name="event">The event instance to publish</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	/// <returns>A task representing the asynchronous publish operation</returns>
	public async Task PublishAsync<TEvent>( TEvent @event, CancellationToken cancellationToken = default )
		where TEvent : IEvent {
		using var activity = _activitySource.StartActivity( $"EventBus.Publish.{typeof( TEvent ).Name}" );

		try {
			var eventType = typeof( TEvent );

			_logger.LogInformation( "Publishing event {EventType} with ID {EventId}",
				eventType.Name, @event.EventId );

			// Only publish to message broker - handlers will be processed by the broker to avoid duplication
			await _messageBroker.PublishAsync( @event, cancellationToken );

			activity?.SetStatus( ActivityStatusCode.Ok );
		} catch ( Exception ex ) {
			activity?.SetStatus( ActivityStatusCode.Error, ex.Message );
			_logger.LogError( ex, "Error publishing event {EventType}", typeof( TEvent ).Name );
			throw;
		}
	}

	/// <summary>
	/// Subscribes a handler to a specific event type
	/// </summary>
	/// <typeparam name="TEvent">The type of event to subscribe to</typeparam>
	/// <typeparam name="THandler">The handler type that will process the event</typeparam>
	public void Subscribe<TEvent, THandler>( )
		where TEvent : IEvent
		where THandler : IEventHandler<TEvent> {
		var eventType = typeof( TEvent );
		var handlerType = typeof( THandler );

		_subscriptions.AddOrUpdate(
			eventType,
			_ => new List<Type> { handlerType },
			( _, handlers ) => {
				if ( !handlers.Contains( handlerType ) )
					handlers.Add( handlerType );
				return handlers;
			} );

		_logger.LogInformation( "Subscribed {HandlerType} to {EventType}",
			handlerType.Name, eventType.Name );
	}

	/// <summary>
	/// Unsubscribes a handler from a specific event type
	/// </summary>
	/// <typeparam name="TEvent">The type of event to unsubscribe from</typeparam>
	/// <typeparam name="THandler">The handler type to remove from subscriptions</typeparam>
	public void Unsubscribe<TEvent, THandler>( )
		where TEvent : IEvent
		where THandler : IEventHandler<TEvent> {
		var eventType = typeof( TEvent );
		var handlerType = typeof( THandler );

		if ( _subscriptions.TryGetValue( eventType, out var handlers ) ) {
			handlers.Remove( handlerType );
			_logger.LogInformation( "Unsubscribed {HandlerType} from {EventType}",
				handlerType.Name, eventType.Name );
		}
	}
}