using Myth.Flow.Interfaces;
using System.Collections.Concurrent;

/// <summary>
/// Default event handler registry implementation
/// </summary>
internal sealed class EventHandlerRegistry : IEventHandlerRegistry {
	private readonly IEventSubscriptionManager _subscriptionManager;
	private readonly ConcurrentBag<(Type EventType, Type HandlerType)> _registrations = new( );

	public EventHandlerRegistry( IEventSubscriptionManager subscriptionManager ) {
		_subscriptionManager = subscriptionManager;
	}

	public void RegisterHandler( Type eventType, Type handlerType ) {
		var registerMethod = typeof( IEventSubscriptionManager )
			.GetMethod( nameof( IEventSubscriptionManager.RegisterHandler ) )
			?.MakeGenericMethod( eventType, handlerType );

		registerMethod?.Invoke( _subscriptionManager, null );

		_registrations.Add( (eventType, handlerType) );
	}

	public IEnumerable<(Type EventType, Type HandlerType)> GetRegisteredHandlers( ) =>
		_registrations.ToList( );
}