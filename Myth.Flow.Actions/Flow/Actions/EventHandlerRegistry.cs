using System.Collections.Concurrent;
using Myth.Interfaces;

namespace Myth.Flow.Actions;

/// <summary>
/// Default event handler registry implementation
/// </summary>
internal sealed class EventHandlerRegistry : IEventHandlerRegistry {
	private readonly IEventSubscriptionManager _subscriptionManager;
	private readonly ConcurrentDictionary<Type, ConcurrentBag<Type>> _registrations = new( );

	public EventHandlerRegistry( IEventSubscriptionManager subscriptionManager ) {
		_subscriptionManager = subscriptionManager;
	}

	/// <summary>
	/// Registers an event handler for a specific event type
	/// </summary>
	/// <param name="eventType">The type of event to handle</param>
	/// <param name="handlerType">The type of handler that processes the event</param>
	public void RegisterHandler( Type eventType, Type handlerType ) {
		var registerMethod = typeof( IEventSubscriptionManager )
			.GetMethod( nameof( IEventSubscriptionManager.RegisterHandler ) )
			?.MakeGenericMethod( eventType, handlerType );

		registerMethod?.Invoke( _subscriptionManager, null );

		_registrations.AddOrUpdate(
			eventType,
			_ => [ handlerType ],
			( _, bag ) => {
				if ( !bag.Contains( handlerType ) )
					bag.Add( handlerType );
				return bag;
			} );
	}

	/// <summary>
	/// Gets all registered event handler pairs
	/// </summary>
	/// <returns>A collection of tuples containing event types and their corresponding handler types</returns>
	public IEnumerable<(Type EventType, Type HandlerType)> GetRegisteredHandlers( ) =>
		_registrations.SelectMany( kvp =>
			kvp.Value.Select( handlerType => (kvp.Key, handlerType) ) ).ToList( );
}
