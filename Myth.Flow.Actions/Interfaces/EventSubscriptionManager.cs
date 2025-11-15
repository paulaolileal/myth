using System.Collections.Concurrent;

namespace Myth.Interfaces;

/// <summary>
/// Default event subscription manager implementation
/// </summary>
internal sealed class EventSubscriptionManager : IEventSubscriptionManager {
	private readonly ConcurrentDictionary<Type, ConcurrentBag<Type>> _handlers = new( );

	/// <summary>
	/// Registers a handler for a specific event type
	/// </summary>
	/// <typeparam name="TEvent">The event type to handle</typeparam>
	/// <typeparam name="THandler">The handler type that processes the event</typeparam>
	public void RegisterHandler<TEvent, THandler>( )
		where TEvent : IEvent
		where THandler : IEventHandler<TEvent> {
		var eventType = typeof( TEvent );
		var handlerType = typeof( THandler );

		_handlers.AddOrUpdate(
			eventType,
			_ => new ConcurrentBag<Type> { handlerType },
			( _, bag ) => {
				if ( !bag.Contains( handlerType ) )
					bag.Add( handlerType );
				return bag;
			} );
	}

	/// <summary>
	/// Gets all handlers registered for a specific event type
	/// </summary>
	/// <typeparam name="TEvent">The event type to query</typeparam>
	/// <returns>A collection of handler types for the event</returns>
	public IEnumerable<Type> GetHandlersForEvent<TEvent>( ) where TEvent : IEvent =>
		GetHandlersForEvent( typeof( TEvent ) );

	/// <summary>
	/// Gets all handlers registered for a specific event type
	/// </summary>
	/// <param name="eventType">The event type to query</param>
	/// <returns>A collection of handler types for the event</returns>
	public IEnumerable<Type> GetHandlersForEvent( Type eventType ) =>
		_handlers.TryGetValue( eventType, out var handlers )
			? handlers.ToList( )
			: Enumerable.Empty<Type>( );

	/// <summary>
	/// Checks if any handlers are registered for a specific event type
	/// </summary>
	/// <typeparam name="TEvent">The event type to check</typeparam>
	/// <returns>True if handlers exist for the event type, otherwise false</returns>
	public bool HasHandlers<TEvent>( ) where TEvent : IEvent =>
		_handlers.TryGetValue( typeof( TEvent ), out var handlers ) && handlers.Any( );
}
