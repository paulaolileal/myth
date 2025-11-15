namespace Myth.Interfaces;

/// <summary>
/// Event bus for publishing and subscribing to events
/// </summary>
public interface IEventBus {

	/// <summary>
	/// Publishes an event
	/// </summary>
	Task PublishAsync<TEvent>( TEvent @event, CancellationToken cancellationToken = default )
		where TEvent : IEvent;

	/// <summary>
	/// Subscribes to an event type
	/// </summary>
	void Subscribe<TEvent, THandler>( )
		where TEvent : IEvent
		where THandler : IEventHandler<TEvent>;

	/// <summary>
	/// Unsubscribes from an event type
	/// </summary>
	void Unsubscribe<TEvent, THandler>( )
		where TEvent : IEvent
		where THandler : IEventHandler<TEvent>;
}
