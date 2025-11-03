namespace Myth.Interfaces;

/// <summary>
/// Scans assemblies for handlers
/// </summary>
public interface IAssemblyScanner {

	/// <summary>
	/// Scans for command and query handlers
	/// </summary>
	IEnumerable<(Type InterfaceType, Type ImplementationType)> ScanForHandlers( params System.Reflection.Assembly[ ] assemblies );

	/// <summary>
	/// Scans for event handlers
	/// </summary>
	IEnumerable<(Type EventType, Type HandlerType)> ScanForEventHandlers( params System.Reflection.Assembly[ ] assemblies );
}