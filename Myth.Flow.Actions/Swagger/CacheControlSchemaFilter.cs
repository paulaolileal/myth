using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Myth.Flow.Actions.ValueObjects;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Myth.Flow.Actions.Swagger;

/// <summary>
/// Schema filter to display CacheControl as enum with predefined values in Swagger UI
/// </summary>
public class CacheControlSchemaFilter : ISchemaFilter {

	public void Apply( OpenApiSchema schema, SchemaFilterContext context ) {
		var targetType = Nullable.GetUnderlyingType( context.Type ) ?? context.Type;
		if ( targetType == typeof( CacheControl ) ) {
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
			schema.Properties?.Clear( );
			schema.AdditionalProperties = null;
		}
	}
}
