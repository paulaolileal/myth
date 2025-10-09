using System.Collections.Concurrent;

namespace Myth.Interfaces;

/// <summary>
/// Default event subscription manager implementation
/// </summary>
internal sealed class EventSubscriptionManager : IEventSubscriptionManager {
	private readonly ConcurrentDictionary<Type, ConcurrentBag<Type>> _handlers = new( );

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

	public IEnumerable<Type> GetHandlersForEvent<TEvent>( ) where TEvent : IEvent =>
		GetHandlersForEvent( typeof( TEvent ) );

	public IEnumerable<Type> GetHandlersForEvent( Type eventType ) =>
		_handlers.TryGetValue( eventType, out var handlers )
			? handlers.ToList( )
			: Enumerable.Empty<Type>( );

	public bool HasHandlers<TEvent>( ) where TEvent : IEvent =>
		_handlers.TryGetValue( typeof( TEvent ), out var handlers ) && handlers.Any( );
}