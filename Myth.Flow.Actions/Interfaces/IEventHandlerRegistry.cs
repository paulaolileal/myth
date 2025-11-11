namespace Myth.Interfaces;

/// <summary>
/// Registry for event handlers
/// </summary>
public interface IEventHandlerRegistry {

	/// <summary>
	/// Registers event handler
	/// </summary>
	void RegisterHandler( Type eventType, Type handlerType );

	/// <summary>
	/// Gets registered handlers
	/// </summary>
	IEnumerable<(Type EventType, Type HandlerType)> GetRegisteredHandlers( );
}
