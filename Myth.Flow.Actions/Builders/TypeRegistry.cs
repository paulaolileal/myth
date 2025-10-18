using System.Collections.Concurrent;

/// <summary>
/// Registry for discovered types
/// </summary>
internal sealed class TypeRegistry {
	private readonly ConcurrentDictionary<Type, List<Type>> _handlerMap = new( );

	/// <summary>
	/// Registers a handler for a specific request type
	/// </summary>
	/// <param name="requestType">The type of request to handle</param>
	/// <param name="handlerType">The type of handler that processes the request</param>
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

	/// <summary>
	/// Gets all handlers registered for a specific request type
	/// </summary>
	/// <param name="requestType">The request type to query</param>
	/// <returns>A collection of handler types for the request</returns>
	public IEnumerable<Type> GetHandlers( Type requestType ) {
		return _handlerMap.TryGetValue( requestType, out var handlers )
			? handlers
			: Enumerable.Empty<Type>( );
	}

	/// <summary>
	/// Checks if any handler is registered for a specific request type
	/// </summary>
	/// <param name="requestType">The request type to check</param>
	/// <returns>True if a handler exists for the request type, otherwise false</returns>
	public bool HasHandler( Type requestType ) {
		return _handlerMap.ContainsKey( requestType );
	}

	/// <summary>
	/// Gets all registered request-handler pairs
	/// </summary>
	/// <returns>A collection of tuples containing request types and their corresponding handler types</returns>
	public IEnumerable<(Type RequestType, Type HandlerType)> GetAllRegistrations( ) {
		foreach ( var kvp in _handlerMap ) {
			foreach ( var handlerType in kvp.Value ) {
				yield return (kvp.Key, handlerType);
			}
		}
	}
}