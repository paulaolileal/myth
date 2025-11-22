using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Myth.Flow.Actions.ValueObjects;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Myth.Flow.Actions.Swagger;

/// <summary>
/// Schema filter to display CacheControl as enum with predefined values in Swagger UI
/// Compatible with multiple Swashbuckle versions
/// </summary>
public class CacheControlSchemaFilter : ISchemaFilter {

	public void Apply( OpenApiSchema schema, SchemaFilterContext context ) {
		try {
			var targetType = GetTargetType( context );
			if ( targetType != typeof( CacheControl ) ) {
				return;
			}

			ConfigureSchema( schema );
		} catch {
			// Fail silently if there are any compatibility issues
			// This ensures the application doesn't crash due to Swagger version mismatches
		}
	}

	private static Type GetTargetType( SchemaFilterContext context ) {
		// Handle nullable types safely
		var contextType = GetContextType( context );
		return Nullable.GetUnderlyingType( contextType ) ?? contextType;
	}

	private static Type GetContextType( SchemaFilterContext context ) {
		try {
			// Try to access Type property directly
			return context.Type;
		} catch {
			// Fallback using reflection for older versions
			try {
				var typeProperty = context.GetType( ).GetProperty( "Type", BindingFlags.Public | BindingFlags.Instance );
				return ( Type )typeProperty?.GetValue( context )!;
			} catch {
				return typeof( object ); // Safe fallback
			}
		}
	}

	private static void ConfigureSchema( OpenApiSchema schema ) {
		schema.Type = "string";
		schema.Format = null;
		schema.Description = "HTTP Cache-Control directive. Use predefined constants or custom values like 'max-age=3600', 'public, max-age=1800', etc.";

		// Add enum values for common cache control directives
		schema.Enum = new List<IOpenApiAny> {
			new OpenApiString( "no-cache" ),
			new OpenApiString( "no-store" ),
			new OpenApiString( "public" ),
			new OpenApiString( "private" ),
			new OpenApiString( "must-revalidate" ),
			new OpenApiString( "proxy-revalidate" ),
			new OpenApiString( "no-transform" ),
			new OpenApiString( "immutable" ),
			new OpenApiString( "max-age=3600" ),
			new OpenApiString( "public, max-age=1800" ),
			new OpenApiString( "private, max-age=300" ),
			new OpenApiString( "public, immutable, max-age=31536000" )
		};

		// Set example value
		schema.Example = new OpenApiString( "no-cache" );

		// Add pattern for validation hint
		schema.Pattern = @"^[a-zA-Z0-9\-=,\s]+$";

		// Remove properties that might interfere with enum display
		try {
			schema.Properties?.Clear( );
			schema.AdditionalProperties = null;
		} catch {
			// Ignore if these properties don't exist in this version
		}
	}
}
