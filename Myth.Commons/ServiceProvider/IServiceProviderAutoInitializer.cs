namespace Myth.ServiceProvider;

/// <summary>
/// Marker interface for service provider auto-initialization.
/// Services implementing this interface are used to trigger automatic
/// initialization of the global service provider when the DI container is built.
/// </summary>
public interface IServiceProviderAutoInitializer {

	/// <summary>
	/// Gets the name of the library that registered this initializer
	/// </summary>
	string LibraryName { get; }
}