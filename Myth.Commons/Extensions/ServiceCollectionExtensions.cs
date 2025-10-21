using Microsoft.Extensions.DependencyInjection;
using Myth.ServiceProvider;

namespace Myth.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> related to Myth service provider management.
/// </summary>
public static class ServiceCollectionExtensions {

	/// <summary>
	/// Adds automatic service provider initialization for Myth libraries.
	/// This method registers a service that will initialize the global service provider
	/// when the DI container is built. Each library should call this method in their
	/// AddXxx() extension method.
	/// </summary>
	/// <param name="services">The service collection</param>
	/// <param name="libraryName">Name of the library registering the auto-initializer</param>
	/// <returns>The same service collection for chaining</returns>
	/// <exception cref="ArgumentNullException">Thrown when services or libraryName is null</exception>
	/// <exception cref="ArgumentException">Thrown when libraryName is empty or whitespace</exception>
	public static IServiceCollection AddMythAutoInitialization( this IServiceCollection services, string libraryName ) {
		if ( services == null )
			throw new ArgumentNullException( nameof( services ) );

		if ( string.IsNullOrWhiteSpace( libraryName ) )
			throw new ArgumentException( "Library name cannot be null or empty", nameof( libraryName ) );

		// Register auto-initializer that will be created when the container is built
		// This ensures the global service provider is initialized automatically
		services.AddSingleton<IServiceProviderAutoInitializer>(
			ServiceProviderHelper.CreateAutoInitializerFactory( libraryName ) );

		return services;
	}

	/// <summary>
	/// Explicitly initializes the Myth global service provider (optional).
	/// This method provides explicit control over when the global service provider
	/// is initialized. It's not required when using AddMythAutoInitialization(),
	/// but can be useful for scenarios requiring explicit initialization timing.
	/// </summary>
	/// <param name="services">The service collection</param>
	/// <returns>The same service collection for chaining</returns>
	/// <exception cref="ArgumentNullException">Thrown when services is null</exception>
	public static IServiceCollection InitializeMythGlobalProvider( this IServiceCollection services ) {
		if ( services == null )
			throw new ArgumentNullException( nameof( services ) );

		// Register a service that will explicitly initialize the global provider
		services.AddSingleton<IGlobalServiceProviderInitializer>( serviceProvider => {
			MythServiceProvider.Initialize( serviceProvider );
			return new GlobalServiceProviderInitializer( );
		} );

		return services;
	}

	/// <summary>
	/// Builds the service provider and ensures it's set as the global Myth provider.
	/// This is a convenience method that combines BuildServiceProvider() with
	/// explicit global provider initialization.
	/// </summary>
	/// <param name="services">The service collection</param>
	/// <returns>The built service provider, which is also set as the global provider</returns>
	/// <exception cref="ArgumentNullException">Thrown when services is null</exception>
	public static IServiceProvider BuildWithMythGlobalProvider( this IServiceCollection services ) {
		if ( services == null )
			throw new ArgumentNullException( nameof( services ) );

		var serviceProvider = services.BuildServiceProvider( );
		MythServiceProvider.Initialize( serviceProvider );
		return serviceProvider;
	}
}

/// <summary>
/// Marker interface for explicit global service provider initialization.
/// Used when developers want explicit control over initialization timing.
/// </summary>
public interface IGlobalServiceProviderInitializer {
}

/// <summary>
/// Default implementation of <see cref="IGlobalServiceProviderInitializer"/>.
/// </summary>
internal class GlobalServiceProviderInitializer : IGlobalServiceProviderInitializer {
}