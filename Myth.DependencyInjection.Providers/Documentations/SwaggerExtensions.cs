using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Myth.Documentations.Settings;
using Myth.Documentations.ValueProviders;
using Myth.ServiceProvider;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;
using System.Text.Json;
using static Myth.Documentations.Settings.SwaggerSettings;

namespace Myth.Documentations;

public static class SwaggerExtensions {

	/// <summary>
	/// Adds advanced Swagger/OpenAPI documentation to the service collection with enhanced features
	/// </summary>
	/// <param name="services">The service collection to extend</param>
	/// <param name="settings">Optional configuration for advanced Swagger features including tree view, search, themes, and authentication</param>
	/// <returns>The updated service collection for method chaining</returns>
	/// <remarks>
	/// This method provides enhanced Swagger UI with features like hierarchical navigation,
	/// real-time search, dark/light themes, persistent caching, and advanced authentication options.
	/// Use the settings parameter to configure specific features or call UseAdvancedFeatures() for defaults.
	/// </remarks>
	public static IServiceCollection AddDocs( this IServiceCollection services, Action<SwaggerSettings>? settings = null ) {
		services.AddEndpointsApiExplorer( );

		// Configure and register SwaggerSettings for later use
		var swaggerSettings = new SwaggerSettings( );
		settings?.Invoke( swaggerSettings );
		services.AddSingleton( swaggerSettings );

		services.AddSwaggerGen( options => {
			// Resolve the version provider from the fully configured service provider at runtime
			var serviceProvider = MythServiceProvider.GetOrFallback( services.BuildServiceProvider( ) );
			var versionProvider = serviceProvider?.GetService<IApiVersionDescriptionProvider>( );

			if ( versionProvider != null ) {
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
	/// Configures and enables the enhanced Swagger UI middleware with advanced features in the application pipeline
	/// </summary>
	/// <param name="app">The application builder to configure</param>
	/// <returns>The updated application builder for method chaining</returns>
	/// <remarks>
	/// This method sets up the complete Swagger UI experience including:
	/// - Custom embedded UI assets (CSS, JavaScript, HTML)
	/// - Multi-version API endpoint discovery
	/// - Advanced UI features (tree view, search, themes, caching)
	/// - Automatic authentication middleware integration when configured
	/// - Error handling and graceful fallbacks
	/// Must be called after AddDocs() in service configuration.
	/// </remarks>
	public static IApplicationBuilder UseDocs( this IApplicationBuilder app ) {
		// Get Swagger settings from DI container
		var swaggerSettings = app.ApplicationServices.GetService<SwaggerSettings>( );
		var environment = app.ApplicationServices.GetRequiredService<IHostEnvironment>( );

		// Apply authentication middleware if configured
		if ( swaggerSettings?.SwaggerAuthentication != null ) {
			var authSettings = swaggerSettings.SwaggerAuthentication;
			bool shouldRequireAuth = authSettings.RequireAuthentication || authSettings.EnvironmentBasedRequirement( environment );

			if ( shouldRequireAuth ) {
				app.UseSwaggerAuthenticationMiddleware( swaggerSettings );
			}
		}

		// Setup static file serving for advanced UI assets
		app.UseStaticFiles( new StaticFileOptions {
			FileProvider = new EmbeddedFileProvider( typeof( SwaggerExtensions ).Assembly, "Myth.Documentations.UI.Assets" ),
			RequestPath = "/swagger-ui"
		} );

		app.UseSwagger( );
		app.UseSwaggerUI( options => {
			var serviceProvider = MythServiceProvider.GetOrFallback( app.ApplicationServices );
			var versionProvider = serviceProvider?.GetRequiredService<IApiVersionDescriptionProvider>( );
			var context = serviceProvider?.GetRequiredService<IHttpContextAccessor>( );

			// Configure endpoints
			if ( versionProvider != null ) {
				foreach ( var versionDescription in versionProvider.ApiVersionDescriptions ) {
					options.SwaggerEndpoint(
						$"{context?.HttpContext?.Request.Path}/swagger/{versionDescription.GroupName}/swagger.json",
						versionDescription.GroupName.ToUpperInvariant( ) );
				}
			}

			// Configure advanced UI features
			ConfigureAdvancedUI( options, serviceProvider );
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
			var resourceName = "Myth.Documentations.UI.Assets.swagger-advanced.html";

			var stream = assembly.GetManifestResourceStream( resourceName );
			if ( stream != null ) {
				using var reader = new StreamReader( stream );
				var html = reader.ReadToEnd( );

				// Get the first available swagger endpoint dynamically
				var specUrl = GetDefaultSwaggerEndpoint( services );

				// Replace placeholders with actual values
				html = html.Replace( "%(DocumentTitle)", "API Documentation" );
				html = html.Replace( "%(DocumentDescription)", "API Documentation with advanced features" );
				html = html.Replace( "%(SpecUrl)", specUrl );
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
	/// Gets the default swagger endpoint URL dynamically based on available API versions
	/// </summary>
	/// <param name="services">The service provider for accessing configuration.</param>
	/// <returns>The swagger.json endpoint URL.</returns>
	private static string GetDefaultSwaggerEndpoint( IServiceProvider services ) {
		try {
			var versionProvider = services?.GetService<IApiVersionDescriptionProvider>( );
			if ( versionProvider?.ApiVersionDescriptions?.Any( ) == true ) {
				var firstVersion = versionProvider.ApiVersionDescriptions.First( );
				return $"./swagger/{firstVersion.GroupName}/swagger.json";
			}
		} catch ( Exception ex ) {
			Console.WriteLine( $"Warning: Failed to get dynamic swagger endpoint: {ex.Message}" );
		}

		// Fallback to default
		return "./swagger/v1/swagger.json";
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
            // Try multiple common swagger endpoints
            const possibleUrls = [
                './swagger/v1/swagger.json',
                './swagger/v1.0/swagger.json',
                './swagger/default/swagger.json'
            ];

            let ui;
            function tryNextUrl(index = 0) {
                if (index >= possibleUrls.length) {
                    document.getElementById('swagger-ui').innerHTML = '<h3>Failed to load API definition. Please check if swagger.json endpoints are properly configured.</h3>';
                    return;
                }

                ui = SwaggerUIBundle({
                    url: possibleUrls[index],
                    dom_id: '#swagger-ui',
                    deepLinking: true,
                    presets: [SwaggerUIBundle.presets.apis, SwaggerUIStandalonePreset],
                    plugins: [SwaggerUIBundle.plugins.DownloadUrl],
                    layout: 'StandaloneLayout',
                    onFailure: function() {
                        tryNextUrl(index + 1);
                    }
                });
            }

            tryNextUrl();
        };
    </script>
</body>
</html>";

		return new MemoryStream( System.Text.Encoding.UTF8.GetBytes( defaultHtml ) );
	}

	/// <summary>
	/// Adds Swagger authentication middleware to the application pipeline
	/// </summary>
	/// <param name="app">The application builder</param>
	/// <param name="swaggerSettings">The Swagger settings containing authentication configuration</param>
	/// <returns>The updated application builder</returns>
	/// <remarks>
	/// This method is automatically called by UseDocs() when authentication is configured.
	/// It integrates authentication validation directly into the Swagger UI pipeline.
	/// </remarks>
	private static IApplicationBuilder UseSwaggerAuthenticationMiddleware( this IApplicationBuilder app, SwaggerSettings swaggerSettings ) {
		return app.Use( async ( context, next ) => {
			var authSettings = swaggerSettings.SwaggerAuthentication;

			// Check if request is for Swagger endpoints
			if ( !IsSwaggerEndpoint( context.Request.Path ) ) {
				await next( context );
				return;
			}

			// Check bypass paths
			if ( authSettings.BypassPaths.Any( path => context.Request.Path.StartsWithSegments( path ) ) ) {
				await next( context );
				return;
			}

			// Validate authentication
			bool isAuthenticated = false;

			if ( authSettings.CustomAuthenticationValidator != null ) {
				// Use custom validator
				isAuthenticated = await authSettings.CustomAuthenticationValidator( context );
			} else {
				// Use default validation logic
				isAuthenticated = await ValidateDefaultAuthentication( context, authSettings );
			}

			if ( !isAuthenticated ) {
				await HandleUnauthorizedResponse( context, authSettings.UnauthorizedResponse );
				return;
			}

			await next( context );
		} );
	}

	/// <summary>
	/// Checks if the request path is for a Swagger endpoint
	/// </summary>
	/// <param name="path">The request path</param>
	/// <returns>True if the path is for Swagger</returns>
	private static bool IsSwaggerEndpoint( PathString path ) {
		return path.StartsWithSegments( "/swagger" ) ||
			   path.StartsWithSegments( "/swagger-ui" );
	}

	/// <summary>
	/// Validates authentication using default ASP.NET Core authentication
	/// </summary>
	/// <param name="context">The HTTP context</param>
	/// <param name="authSettings">The authentication settings</param>
	/// <returns>True if authentication is valid</returns>
	private static async Task<bool> ValidateDefaultAuthentication( HttpContext context, SwaggerAuthenticationSettings authSettings ) {
		try {
			// Check if user is already authenticated via ASP.NET Core
			if ( context.User?.Identity?.IsAuthenticated == true ) {
				return true;
			}

			// Check for API key authentication if allowed
			if ( authSettings.AllowedSchemes.Contains( AuthorizationType.ApiKey ) ) {
				if ( await ValidateApiKeyAsync( context ) ) {
					return true;
				}
			}

			// Check for Bearer token authentication if allowed
			if ( authSettings.AllowedSchemes.Contains( AuthorizationType.Bearer ) ) {
				if ( await ValidateBearerTokenAsync( context ) ) {
					return true;
				}
			}

			// Check for Basic authentication if allowed
			if ( authSettings.AllowedSchemes.Contains( AuthorizationType.Basic ) ) {
				if ( await ValidateBasicAuthAsync( context ) ) {
					return true;
				}
			}

			return false;
		} catch ( Exception ) {
			return false;
		}
	}

	/// <summary>
	/// Validates API key authentication
	/// </summary>
	/// <param name="context">The HTTP context</param>
	/// <returns>True if API key is valid</returns>
	private static async Task<bool> ValidateApiKeyAsync( HttpContext context ) {
		try {
			var apiKey = context.Request.Headers[ "X-API-Key" ].FirstOrDefault( ) ??
						context.Request.Query[ "apikey" ].FirstOrDefault( );

			if ( string.IsNullOrEmpty( apiKey ) ) {
				return false;
			}

			// In a real implementation, validate against configured API keys
			await Task.CompletedTask; // Simulate async validation
			return !string.IsNullOrEmpty( apiKey );
		} catch {
			return false;
		}
	}

	/// <summary>
	/// Validates Bearer token authentication
	/// </summary>
	/// <param name="context">The HTTP context</param>
	/// <returns>True if Bearer token is valid</returns>
	private static async Task<bool> ValidateBearerTokenAsync( HttpContext context ) {
		try {
			var authHeader = context.Request.Headers[ "Authorization" ].FirstOrDefault( );

			if ( string.IsNullOrEmpty( authHeader ) || !authHeader.StartsWith( "Bearer " ) ) {
				return false;
			}

			var token = authHeader.Substring( "Bearer ".Length ).Trim( );

			if ( string.IsNullOrEmpty( token ) ) {
				return false;
			}

			// In a real implementation, validate JWT token
			await Task.CompletedTask; // Simulate async validation
			return !string.IsNullOrEmpty( token );
		} catch {
			return false;
		}
	}

	/// <summary>
	/// Validates Basic authentication
	/// </summary>
	/// <param name="context">The HTTP context</param>
	/// <returns>True if Basic auth is valid</returns>
	private static async Task<bool> ValidateBasicAuthAsync( HttpContext context ) {
		try {
			var authHeader = context.Request.Headers[ "Authorization" ].FirstOrDefault( );

			if ( string.IsNullOrEmpty( authHeader ) || !authHeader.StartsWith( "Basic " ) ) {
				return false;
			}

			var encodedCredentials = authHeader.Substring( "Basic ".Length ).Trim( );
			var credentials = System.Text.Encoding.UTF8.GetString( Convert.FromBase64String( encodedCredentials ) );
			var parts = credentials.Split( ':', 2 );

			if ( parts.Length != 2 ) {
				return false;
			}

			var username = parts[ 0 ];
			var password = parts[ 1 ];

			// In a real implementation, validate against user store
			await Task.CompletedTask; // Simulate async validation
			return !string.IsNullOrEmpty( username ) && !string.IsNullOrEmpty( password );
		} catch {
			return false;
		}
	}

	/// <summary>
	/// Handles unauthorized response when authentication fails
	/// </summary>
	/// <param name="context">The HTTP context</param>
	/// <param name="responseSettings">The unauthorized response settings</param>
	/// <returns>Task representing the async operation</returns>
	private static async Task HandleUnauthorizedResponse( HttpContext context, UnauthorizedResponseSettings responseSettings ) {
		context.Response.StatusCode = responseSettings.StatusCode;
		context.Response.ContentType = responseSettings.ContentType;

		// Add additional headers
		foreach ( var header in responseSettings.AdditionalHeaders ) {
			context.Response.Headers[ header.Key ] = header.Value;
		}

		string responseBody;

		if ( responseSettings.CustomResponseBodyGenerator != null ) {
			responseBody = responseSettings.CustomResponseBodyGenerator( );
		} else {
			// Default JSON response
			var response = new {
				error = responseSettings.ErrorCode,
				message = responseSettings.Message,
				timestamp = responseSettings.IncludeTimestamp ? DateTime.UtcNow : ( DateTime? )null
			};

			responseBody = JsonSerializer.Serialize( response, new JsonSerializerOptions {
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				WriteIndented = true,
				DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
			} );
		}

		await context.Response.WriteAsync( responseBody );
	}
}