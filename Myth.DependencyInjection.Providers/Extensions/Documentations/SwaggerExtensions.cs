using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using Myth.Extensions.Swagger.Settings;
using Myth.ValueProviders;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;
using System.Text.Json;
using static Myth.Extensions.Swagger.Settings.SwaggerSettings;

namespace Myth.Extensions.Swagger;

public static class SwaggerExtensions {

	public static IServiceCollection AddDocs( this IServiceCollection services, Action<SwaggerSettings>? settings = null ) {
		var serviceProvider = services.BuildServiceProvider( );
		var versionProvider = serviceProvider.GetRequiredService<IApiVersionDescriptionProvider>( );

		services.AddEndpointsApiExplorer( );

		services.AddSwaggerGen( options => {
			var swaggerSettings = new SwaggerSettings( );
			settings?.Invoke( swaggerSettings );

			foreach ( var versionDescription in versionProvider.ApiVersionDescriptions ) {
				var info = new OpenApiInfo {
					Version = versionDescription.ApiVersion.ToString( ),
					Title = swaggerSettings.Title,
					Description = swaggerSettings.Description
				};

				if ( versionDescription.IsDeprecated )
					info.Description += " | This version of API is obsolete.";

				if ( swaggerSettings.ContactName is not null ) {
					var contact = new OpenApiContact {
						Name = swaggerSettings.ContactName,
						Email = swaggerSettings.ContactEmail,
						Url = new Uri( swaggerSettings.ContactUrl )
					};

					info.Contact = contact;
				}

				options.SwaggerDoc( versionDescription.GroupName, info );
			}

			options.EnableAnnotations( );

			var xmlFilename = $"{Assembly.GetEntryAssembly( )!.GetName( ).Name}.xml";
			options.IncludeXmlComments( Path.Combine( AppContext.BaseDirectory, xmlFilename ) );

			switch ( swaggerSettings.Type ) {
				case AuthorizationType.Basic:
				options.UseBasicAuthorization( );
				break;

				case AuthorizationType.Bearer:
				options.UseBearerAuthorization( );
				break;

				case AuthorizationType.ApiKey:
				options.UseApiKeyAuthorization( swaggerSettings );
				break;
			}

			options.OperationFilter<SwaggerValueOperationFilter>( );
		} );

		return services;
	}

	/// <summary>
	/// Configures SwaggerGenOptions to use Basic authentication.
	/// </summary>
	/// <param name="options">The SwaggerGenOptions instance.</param>
	private static void UseBasicAuthorization( this SwaggerGenOptions options ) {
		options.AddSecurityDefinition( "basic", new OpenApiSecurityScheme {
			Name = "Authorization",
			Type = SecuritySchemeType.Http,
			Scheme = "basic",
			In = ParameterLocation.Header,
			Description = "Basic Authorization header using the Bearer scheme."
		} );

		options.AddSecurityRequirement( new OpenApiSecurityRequirement
		{
				{
					new OpenApiSecurityScheme
					{
						Reference = new OpenApiReference
						{
							Type = ReferenceType.SecurityScheme,
							Id = "basic"
						}
					},
					Array.Empty<string>()
				}
			} );
	}

	/// <summary>
	/// Configures SwaggerGenOptions to use JWT Bearer authentication.
	/// </summary>
	/// <param name="options">The SwaggerGenOptions instance.</param>
	private static void UseBearerAuthorization( this SwaggerGenOptions options ) {
		options.AddSecurityDefinition( "Bearer", new OpenApiSecurityScheme {
			Name = "Authorization",
			Type = SecuritySchemeType.Http,
			Scheme = "Bearer",
			In = ParameterLocation.Header,
			BearerFormat = "JWT",
			Description = "Please insert JWT with Bearer into field"
		} );

		options.AddSecurityRequirement( new OpenApiSecurityRequirement
		{
				{
					new OpenApiSecurityScheme
					{
						Reference = new OpenApiReference
						{
							Type = ReferenceType.SecurityScheme,
							Id = "Bearer"
						}
					},
					Array.Empty<string>()
				}
			} );
	}

	/// <summary>
	/// Configures SwaggerGenOptions to use API Key authentication.
	/// </summary>
	/// <param name="options">The SwaggerGenOptions instance.</param>
	/// <param name="settings">The SwaggerSettings instance for API Key configuration.</param>
	private static void UseApiKeyAuthorization( this SwaggerGenOptions options, SwaggerSettings settings ) {
		var location = settings.Authentication.ApiKey.Location switch {
			ApiKeyLocation.Header => ParameterLocation.Header,
			ApiKeyLocation.Query => ParameterLocation.Query,
			ApiKeyLocation.Cookie => ParameterLocation.Cookie,
			_ => ParameterLocation.Header
		};

		options.AddSecurityDefinition( "ApiKey", new OpenApiSecurityScheme {
			Name = settings.Authentication.ApiKey.ParameterName,
			Type = SecuritySchemeType.ApiKey,
			In = location,
			Description = $"API Key authorization using the {location.ToString( ).ToLower( )} parameter '{settings.Authentication.ApiKey.ParameterName}'"
		} );

		options.AddSecurityRequirement( new OpenApiSecurityRequirement
		{
				{
					new OpenApiSecurityScheme
					{
						Reference = new OpenApiReference
						{
							Type = ReferenceType.SecurityScheme,
							Id = "ApiKey"
						}
					},
					Array.Empty<string>()
				}
			} );
	}

	/// <summary>
	/// Applies the previously configured Swagger to the application, enabling Swagger and Swagger UI endpoints.
	/// </summary>
	/// <param name="app">The application builder.</param>
	/// <returns>The updated application builder.</returns>
	public static IApplicationBuilder UseDocs( this IApplicationBuilder app ) {
		// Setup static file serving for advanced UI assets
		app.UseStaticFiles( new StaticFileOptions {
			FileProvider = new EmbeddedFileProvider( typeof( SwaggerExtensions ).Assembly, "Myth.Extensions.Swagger.UI.Assets" ),
			RequestPath = "/swagger-ui"
		} );

		app.UseSwagger( );
		app.UseSwaggerUI( options => {
			var versionProvider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>( );
			var context = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>( );

			// Configure endpoints
			foreach ( var versionDescription in versionProvider.ApiVersionDescriptions ) {
				options.SwaggerEndpoint(
					$"{context.HttpContext?.Request.Path}/swagger/{versionDescription.GroupName}/swagger.json",
					versionDescription.GroupName.ToUpperInvariant( ) );
			}

			// Configure advanced UI features
			ConfigureAdvancedUI( options, app.ApplicationServices );
		} );

		return app;
	}

	/// <summary>
	/// Configures advanced UI features for Swagger UI
	/// </summary>
	/// <param name="options">The SwaggerUIOptions instance.</param>
	/// <param name="services">The service provider for accessing configuration.</param>
	private static void ConfigureAdvancedUI( SwaggerUIOptions options, IServiceProvider services ) {
		try {
			// Inject custom CSS
			options.InjectStylesheet( "/swagger-ui/swagger-advanced.css" );

			// Inject custom JavaScript
			options.InjectJavascript( "/swagger-ui/swagger-advanced.js" );

			// Use custom HTML template
			options.IndexStream = ( ) => GetCustomIndexStream( services );

			// Configure UI settings
			options.EnableDeepLinking( );
			options.EnableFilter( );
			options.EnableValidator( );
			options.DocExpansion( DocExpansion.None );
			options.DefaultModelsExpandDepth( 1 );
			options.DefaultModelExpandDepth( 1 );

			// Configure OAuth if needed
			// This will be expanded based on settings
			// options.OAuthClientId("your-client-id");
			// options.OAuthRealm("your-realm");
			// options.OAuthAppName("your-app-name");
		} catch ( Exception ex ) {
			// Log error but don't break the application
			Console.WriteLine( $"Warning: Failed to configure advanced Swagger UI features: {ex.Message}" );
		}
	}

	/// <summary>
	/// Gets the custom HTML index stream with enhanced features
	/// </summary>
	/// <param name="services">The service provider for accessing configuration.</param>
	/// <returns>Stream containing the custom HTML.</returns>
	private static Stream GetCustomIndexStream( IServiceProvider services ) {
		try {
			// Try to get the embedded HTML template
			var assembly = typeof( SwaggerExtensions ).Assembly;
			var resourceName = "Myth.Extensions.Swagger.UI.Assets.swagger-advanced.html";

			var stream = assembly.GetManifestResourceStream( resourceName );
			if ( stream != null ) {
				using var reader = new StreamReader( stream );
				var html = reader.ReadToEnd( );

				// Replace placeholders with actual values
				html = html.Replace( "%(DocumentTitle)", "API Documentation" );
				html = html.Replace( "%(DocumentDescription)", "API Documentation with advanced features" );
				html = html.Replace( "%(SpecUrl)", "./swagger/v1/swagger.json" );
				html = html.Replace( "%(SwaggerAdvancedConfig)", GetAdvancedConfigJson( services ) );
				html = html.Replace( "%(AnalyticsScript)", "" );
				html = html.Replace( "%(CustomScripts)", "" );

				return new MemoryStream( System.Text.Encoding.UTF8.GetBytes( html ) );
			}
		} catch ( Exception ex ) {
			Console.WriteLine( $"Warning: Failed to load custom Swagger template: {ex.Message}" );
		}

		// Fallback to default template
		return GetDefaultIndexStream( );
	}

	/// <summary>
	/// Gets the configuration JSON for advanced features
	/// </summary>
	/// <param name="services">The service provider for accessing configuration.</param>
	/// <returns>JSON configuration string.</returns>
	private static string GetAdvancedConfigJson( IServiceProvider services ) {
		try {
			// This would be expanded to read actual SwaggerSettings
			// For now, return default configuration
			var config = new {
				treeView = new {
					enableHierarchy = true,
					tagSeparator = "/",
					enableCollapse = true,
					expandByDefault = false,
					showEndpointCount = true
				},
				search = new {
					enableRealTime = true,
					minSearchLength = 2,
					debounceMs = 300,
					enableHighlighting = true,
					caseSensitive = false
				},
				theme = new {
					defaultTheme = "auto",
					allowUserToggle = true,
					persistPreference = true,
					enableTransitions = true
				},
				cache = new {
					enablePersistence = true,
					expirationMinutes = 60,
					enableHistory = true,
					keyPrefix = "swagger_cache_"
				},
				ui = new {
					enableKeyboardShortcuts = true,
					enableDirectExecution = true,
					enableJsonBeautify = true,
					enableModelCollapse = true
				},
				performance = new {
					enableTiming = true,
					enableStatusColors = true,
					enableProgressIndicators = true,
					showToasts = false
				}
			};

			return JsonSerializer.Serialize( config, new JsonSerializerOptions {
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				WriteIndented = false
			} );
		} catch ( Exception ex ) {
			Console.WriteLine( $"Warning: Failed to serialize advanced config: {ex.Message}" );
			return "{}";
		}
	}

	/// <summary>
	/// Gets the default Swagger UI index stream as fallback
	/// </summary>
	/// <returns>Default HTML stream.</returns>
	private static Stream GetDefaultIndexStream( ) {
		const string defaultHtml = @"<!DOCTYPE html>
<html>
<head>
    <title>API Documentation</title>
    <link rel='stylesheet' type='text/css' href='./swagger-ui-bundle.css' />
    <link rel='stylesheet' type='text/css' href='./swagger-ui-standalone-preset.css' />
</head>
<body>
    <div id='swagger-ui'></div>
    <script src='./swagger-ui-bundle.js'></script>
    <script src='./swagger-ui-standalone-preset.js'></script>
    <script>
        window.onload = function() {
            const ui = SwaggerUIBundle({
                url: './swagger/v1/swagger.json',
                dom_id: '#swagger-ui',
                deepLinking: true,
                presets: [SwaggerUIBundle.presets.apis, SwaggerUIStandalonePreset],
                plugins: [SwaggerUIBundle.plugins.DownloadUrl],
                layout: 'StandaloneLayout'
            });
        };
    </script>
</body>
</html>";

		return new MemoryStream( System.Text.Encoding.UTF8.GetBytes( defaultHtml ) );
	}
}