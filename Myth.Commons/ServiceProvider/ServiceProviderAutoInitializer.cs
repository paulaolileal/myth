namespace Myth.ServiceProvider;

/// <summary>
/// Default implementation of <see cref="IServiceProviderAutoInitializer"/>.
/// Used internally by Myth libraries to trigger automatic service provider initialization.
/// </summary>
internal class ServiceProviderAutoInitializer : IServiceProviderAutoInitializer {

	/// <summary>
	/// Initializes a new instance of the <see cref="ServiceProviderAutoInitializer"/> class.
	/// </summary>
	/// <param name="libraryName">The name of the library that registered this initializer</param>
	public ServiceProviderAutoInitializer( string libraryName ) {
		LibraryName = libraryName ?? throw new ArgumentNullException( nameof( libraryName ) );
	}

	/// <summary>
	/// Gets the name of the library that registered this initializer
	/// </summary>
	public string LibraryName { get; }
}