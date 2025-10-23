using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Myth.ServiceProvider;

namespace Myth.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> and <see cref="WebApplicationBuilder"/>
/// related to service provider management and application building.
/// </summary>
public static class ServiceCollectionExtensions {

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
}