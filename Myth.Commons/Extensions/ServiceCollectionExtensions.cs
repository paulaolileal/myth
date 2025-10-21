using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Myth.ServiceProvider;

namespace Myth.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> and <see cref="WebApplicationBuilder"/>
/// related to service provider management and application building.
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
		ArgumentNullException.ThrowIfNull( services );

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
		ArgumentNullException.ThrowIfNull( services );

		// Register a service that will explicitly initialize the global provider
		services.AddSingleton<IGlobalServiceProviderInitializer>( serviceProvider => {
			MythServiceProvider.Initialize( serviceProvider );
			return new GlobalServiceProviderInitializer( );
		} );

		return services;
	}

	/// <summary>
	/// Builds the service provider and initializes the global provider for library integration.
	/// Use this when you need a service provider with cross-library dependency resolution
	/// outside of ASP.NET Core applications.
	/// </summary>
	/// <param name="services">The service collection</param>
	/// <returns>The built service provider with global provider initialized</returns>
	/// <exception cref="ArgumentNullException">Thrown when services is null</exception>
	/// <remarks>
	/// For ASP.NET Core applications, use builder.BuildApp() instead.
	/// This method is intended for console applications, background services,
	/// or other scenarios where you manually build service providers.
	/// </remarks>
	/// <example>
	/// <code>
	/// // Console application or background service
	/// var services = new ServiceCollection();
	/// services.AddFlow();
	/// services.AddGuard();
	///
	/// var serviceProvider = services.BuildWithGlobalProvider();
	///
	/// // Now all libraries can resolve dependencies from each other
	/// var pipeline = Pipeline.Start(context); // Works!
	/// </code>
	/// </example>
	public static IServiceProvider BuildWithGlobalProvider( this IServiceCollection services ) {
		ArgumentNullException.ThrowIfNull( services );

		var serviceProvider = services.BuildServiceProvider( );
		MythServiceProvider.Initialize( serviceProvider );
		return serviceProvider;
	}

	#region WebApplicationBuilder Extensions

	/// <summary>
	/// Builds the web application with automatic library integration.
	/// This method replaces the standard Build() method and ensures that all
	/// registered libraries can properly resolve dependencies from each other.
	/// </summary>
	/// <param name="builder">The web application builder</param>
	/// <returns>The configured web application with library integration enabled</returns>
	/// <exception cref="ArgumentNullException">Thrown when builder is null</exception>
	/// <remarks>
	/// <para>
	/// Use this method instead of builder.Build() when your application uses
	/// multiple integrated libraries that need to share dependencies.
	/// </para>
	/// <para>
	/// This method automatically initializes the global service provider,
	/// enabling cross-library dependency resolution without additional configuration.
	/// </para>
	/// </remarks>
	/// <example>
	/// <code>
	/// var builder = WebApplication.CreateBuilder(args);
	///
	/// builder.Services.AddFlow();
	/// builder.Services.AddGuard();
	/// builder.Services.AddFlowActions(config => { ... });
	///
	/// var app = builder.BuildApp(); // Instead of builder.Build()
	///
	/// app.UseGuard();
	/// app.Run();
	/// </code>
	/// </example>
	public static WebApplication BuildApp( this WebApplicationBuilder builder ) {
		ArgumentNullException.ThrowIfNull( builder );

		var app = builder.Build( );
		MythServiceProvider.Initialize( app.Services );
		return app;
	}

	#endregion

	#region External API for Non-Myth Libraries

	/// <summary>
	/// Provides easy access to the global service provider for external libraries
	/// or code that wants to integrate with the centralized dependency resolution.
	/// </summary>
	/// <remarks>
	/// This method allows any code to access the global service provider that was
	/// initialized by BuildApp() or BuildWithGlobalProvider(), enabling integration
	/// with libraries outside the Myth ecosystem.
	/// </remarks>
	/// <example>
	/// <code>
	/// // In a third-party library or external code
	/// public class ExternalService {
	///     public void DoSomething() {
	///         var serviceProvider = ServiceCollectionExtensions.GetGlobalProvider();
	///         var validator = serviceProvider?.GetService&lt;IValidator&gt;();
	///         // Use any registered service from the global provider
	///     }
	/// }
	/// </code>
	/// </example>
	/// <returns>
	/// The global service provider if initialized, null otherwise.
	/// </returns>
	public static IServiceProvider? GetGlobalProvider( ) {
		return MythServiceProvider.Current;
	}

	/// <summary>
	/// Allows external code to manually initialize the global service provider.
	/// This is useful when integrating with non-Myth libraries or legacy code.
	/// </summary>
	/// <param name="serviceProvider">The service provider to set as global</param>
	/// <exception cref="ArgumentNullException">Thrown when serviceProvider is null</exception>
	/// <remarks>
	/// This method provides a way for external libraries to participate in
	/// the global service provider pattern without depending on Myth infrastructure.
	/// </remarks>
	/// <example>
	/// <code>
	/// // In external integration code
	/// var services = new ServiceCollection();
	/// // Configure services...
	/// var provider = services.BuildServiceProvider();
	///
	/// ServiceCollectionExtensions.InitializeGlobalProvider(provider);
	/// // Now all Myth libraries can access this provider
	/// </code>
	/// </example>
	public static void InitializeGlobalProvider( IServiceProvider serviceProvider ) {
		ArgumentNullException.ThrowIfNull( serviceProvider );

		MythServiceProvider.Initialize( serviceProvider );
	}

	#endregion
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