using System.Collections.Concurrent;

/// <summary>
/// Registry for discovered types
/// </summary>
internal sealed class TypeRegistry {
	private readonly ConcurrentDictionary<Type, List<Type>> _handlerMap = new( );

	public void Register( Type requestType, Type handlerType ) {
		_handlerMap.AddOrUpdate(
			requestType,
			_ => new List<Type> { handlerType },
			( _, list ) => {
				if ( !list.Contains( handlerType ) )
					list.Add( handlerType );
				return list;
			} );
	}

	public IEnumerable<Type> GetHandlers( Type requestType ) {
		return _handlerMap.TryGetValue( requestType, out var handlers )
			? handlers
			: Enumerable.Empty<Type>( );
	}

	public bool HasHandler( Type requestType ) {
		return _handlerMap.ContainsKey( requestType );
	}

	public IEnumerable<(Type RequestType, Type HandlerType)> GetAllRegistrations( ) {
		foreach ( var kvp in _handlerMap ) {
			foreach ( var handlerType in kvp.Value ) {
				yield return (kvp.Key, handlerType);
			}
		}
	}
}