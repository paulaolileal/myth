namespace Myth.ServiceProvider;

/// <summary>
/// Global service provider for all Myth libraries.
/// Provides centralized service provider management with thread-safe initialization.
/// </summary>
public static class MythServiceProvider {
	private static volatile IServiceProvider? _serviceProvider;
	private static readonly object _lock = new( );

	/// <summary>
	/// Gets the current global service provider, or null if not initialized
	/// </summary>
	public static IServiceProvider? Current => _serviceProvider;

	/// <summary>
	/// Checks if the global service provider is initialized
	/// </summary>
	public static bool IsInitialized => _serviceProvider != null;

	/// <summary>
	/// Initializes the global service provider if not already initialized.
	/// Uses first-wins pattern - subsequent calls are ignored.
	/// </summary>
	/// <param name="serviceProvider">The service provider to set as global</param>
	/// <returns>True if initialized by this call, false if already initialized</returns>
	/// <exception cref="ArgumentNullException">Thrown when serviceProvider is null</exception>
	public static bool TryInitialize( IServiceProvider serviceProvider ) {
		if ( serviceProvider == null )
			throw new ArgumentNullException( nameof( serviceProvider ) );

		if ( _serviceProvider != null )
			return false; // Already initialized

		lock ( _lock ) {
			if ( _serviceProvider != null )
				return false; // Double-check after lock

			_serviceProvider = serviceProvider;
			return true;
		}
	}

	/// <summary>
	/// Forces initialization of the global service provider (overwrites existing).
	/// Use with caution - this will replace any existing global provider.
	/// </summary>
	/// <param name="serviceProvider">The service provider to set as global</param>
	/// <exception cref="ArgumentNullException">Thrown when serviceProvider is null</exception>
	public static void Initialize( IServiceProvider serviceProvider ) {
		if ( serviceProvider == null )
			throw new ArgumentNullException( nameof( serviceProvider ) );

		lock ( _lock ) {
			_serviceProvider = serviceProvider;
		}
	}

	/// <summary>
	/// Gets the current service provider or throws if not initialized
	/// </summary>
	/// <returns>The global service provider</returns>
	/// <exception cref="InvalidOperationException">Thrown when no global service provider is initialized</exception>
	public static IServiceProvider GetRequired( ) {
		return _serviceProvider ?? throw new InvalidOperationException(
			"Global service provider not initialized. This usually happens when no Myth library has been registered with DI container yet." );
	}

	/// <summary>
	/// Gets the global service provider or uses the provided fallback
	/// </summary>
	/// <param name="fallbackServiceProvider">Service provider to use if global not available</param>
	/// <returns>Global service provider if available, otherwise the fallback</returns>
	/// <exception cref="InvalidOperationException">Thrown when neither global nor fallback providers are available</exception>
	public static IServiceProvider GetOrFallback( IServiceProvider? fallbackServiceProvider ) {
		return _serviceProvider ?? fallbackServiceProvider ??
			   throw new InvalidOperationException( "No service provider available (global or fallback)" );
	}

	/// <summary>
	/// Resets the global service provider to null.
	/// Primarily intended for testing scenarios.
	/// </summary>
	public static void Reset( ) {
		lock ( _lock ) {
			_serviceProvider = null;
		}
	}
}
