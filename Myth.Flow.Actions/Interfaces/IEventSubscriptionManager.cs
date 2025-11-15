namespace Myth.Interfaces;

/// <summary>
/// Manages event subscriptions
/// </summary>
public interface IEventSubscriptionManager {

	/// <summary>
	/// Registers event handler
	/// </summary>
	void RegisterHandler<TEvent, THandler>( )
		where TEvent : IEvent
		where THandler : IEventHandler<TEvent>;

	/// <summary>
	/// Gets handlers for event type
	/// </summary>
	IEnumerable<Type> GetHandlersForEvent<TEvent>( ) where TEvent : IEvent;

	/// <summary>
	/// Gets handlers for event type
	/// </summary>
	IEnumerable<Type> GetHandlersForEvent( Type eventType );

	/// <summary>
	/// Checks if event has handlers
	/// </summary>
	bool HasHandlers<TEvent>( ) where TEvent : IEvent;
}
