namespace Myth.ServiceProvider;

/// <summary>
/// Helper class for auto-initialization and service provider management in Myth libraries.
/// Provides common patterns for ensuring global service provider initialization.
/// </summary>
internal static class ServiceProviderHelper {

	/// <summary>
	/// Ensures the global service provider is initialized, using the provided one if needed.
	/// This method implements the auto-initialization pattern used by Myth libraries.
	/// </summary>
	/// <param name="serviceProvider">Fallback service provider to use if global not initialized</param>
	/// <returns>The global service provider (initialized or existing)</returns>
	/// <exception cref="ArgumentNullException">Thrown when serviceProvider is null</exception>
	public static IServiceProvider EnsureGlobalProvider( IServiceProvider serviceProvider ) {
		ArgumentNullException.ThrowIfNull( serviceProvider );

		// Try to initialize with the provided service provider
		MythServiceProvider.TryInitialize( serviceProvider );

		// Return the current global provider (either the one we just set, or the existing one)
		return MythServiceProvider.GetRequired( );
	}

	/// <summary>
	/// Gets the global service provider or uses the fallback.
	/// Commonly used in library extension methods to provide seamless fallback behavior.
	/// </summary>
	/// <param name="fallbackServiceProvider">Service provider to use if global not available</param>
	/// <returns>Global service provider if available, otherwise the fallback</returns>
	/// <exception cref="InvalidOperationException">Thrown when neither global nor fallback providers are available</exception>
	public static IServiceProvider GetProviderOrFallback( IServiceProvider? fallbackServiceProvider ) {
		return MythServiceProvider.GetOrFallback( fallbackServiceProvider );
	}

	/// <summary>
	/// Creates an auto-initializer factory function for use in service registration.
	/// This factory ensures the global service provider is initialized when the DI container is built.
	/// </summary>
	/// <param name="libraryName">Name of the library for debugging/logging purposes</param>
	/// <returns>Factory function that can be used in AddSingleton registration</returns>
	public static Func<IServiceProvider, IServiceProviderAutoInitializer> CreateAutoInitializerFactory( string libraryName ) {
		return serviceProvider => {
			// When the container is built, this factory will be called
			// and automatically initialize the global provider
			EnsureGlobalProvider( serviceProvider );
			return new ServiceProviderAutoInitializer( libraryName );
		};
	}
}