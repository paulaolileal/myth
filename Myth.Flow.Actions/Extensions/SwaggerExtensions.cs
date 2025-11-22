using Microsoft.Extensions.DependencyInjection;
using Myth.Flow.Actions.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Myth.Flow.Actions.Extensions;

/// <summary>
/// Extensions for Swagger configuration
/// </summary>
public static class SwaggerExtensions {

	/// <summary>
	/// Adds CacheControl schema filter to display cache directives as dropdown in Swagger UI
	/// Safe for all Swashbuckle versions
	/// </summary>
	/// <param name="options">SwaggerGenOptions to configure</param>
	/// <returns>SwaggerGenOptions for method chaining</returns>
	public static SwaggerGenOptions AddCacheControlSchemaFilter( this SwaggerGenOptions options ) {
		try {
			options.SchemaFilter<CacheControlSchemaFilter>( );
		} catch {
			// Fail silently if there are compatibility issues
			// This ensures the application doesn't crash due to Swagger version mismatches
		}
		return options;
	}

	/// <summary>
	/// Adds all Flow.Actions schema filters for better Swagger UI experience
	/// Safe for all Swashbuckle versions
	/// </summary>
	/// <param name="options">SwaggerGenOptions to configure</param>
	/// <returns>SwaggerGenOptions for method chaining</returns>
	public static SwaggerGenOptions AddFlowActionsSchemaFilters( this SwaggerGenOptions options ) {
		try {
			options.AddCacheControlSchemaFilter( );
		} catch {
			// Fail silently if there are compatibility issues
		}
		return options;
	}
}
