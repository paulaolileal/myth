using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Myth.DependencyInjection.Providers.Models;
using Myth.DependencyInjection.Providers.ValueProviders;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Myth.DependencyInjection.Providers.Extensions {

	public static class SwaggerExtensions {

		/// <summary>
		/// Add a pre-configured Swagger to the application service collections
		/// </summary>
		/// <param name="services">The application service collection</param>
		/// <param name="swaggerOptions">Addtional Swagger options</param>
		/// <returns></returns>
		/// <remarks>
		/// The data must be created in App Settings
		/// </remarks>
		public static IServiceCollection AddSwaggerVersioned( this IServiceCollection services, Action<SwaggerSettings> swaggerSettings ) {
			services.AddEndpointsApiExplorer( );

			var serviceProvider = services.BuildServiceProvider( );
			var versionProvider = serviceProvider.GetRequiredService<IApiVersionDescriptionProvider>( );

			services.AddSwaggerGen( swaggerConfig => {
				var settings = new SwaggerSettings( swaggerConfig );
				swaggerSettings.Invoke( settings );

				foreach ( var versionDescription in versionProvider.ApiVersionDescriptions ) {
					var info = new OpenApiInfo {
						Version = versionDescription.ApiVersion.ToString( ),
						Title = settings.Title,
						Description = settings.Description
					};

					if ( versionDescription.IsDeprecated )
						info.Description += $" | {settings.DeprecatedDescription}";

					var contact = new OpenApiContact {
						Name = settings.ContactName,
						Email = settings.ContactEmail,
						Url = new Uri( settings.ContactUrl )
					};

					info.Contact = contact;

					swaggerConfig.SwaggerDoc( versionDescription.GroupName, info );
				}

				swaggerConfig.EnableAnnotations( );

				var xmlFilename = $"{Assembly.GetEntryAssembly( )!.GetName( ).Name}.xml";
				swaggerConfig.IncludeXmlComments( Path.Combine( AppContext.BaseDirectory, xmlFilename ) );

				swaggerConfig.OperationFilter<SwaggerValueOperationFilter>( );
			} );

			return services;
		}

		/// <summary>
		/// Add a pre-configured Swagger to the application service collections with basic authorization
		/// </summary>
		/// <param name="services">The application service collection</param>
		/// <returns></returns>
		/// <remarks>
		/// The data must be created in App Settings
		/// </remarks>
		public static SwaggerGenOptions UseBasicAuthorization( this SwaggerGenOptions options ) {
			options.AddSecurityDefinition( "basic", new OpenApiSecurityScheme {
				Name = "Authorization",
				Type = SecuritySchemeType.Http,
				Scheme = "basic",
				In = ParameterLocation.Header,
				Description = "Basic Authorization header using the Bearer scheme."
			} );

			options.AddSecurityRequirement( new OpenApiSecurityRequirement {
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

			return options;
		}

		/// <summary>
		/// Uses the pre configurated Swagger previously added
		/// </summary>
		/// <param name="app"></param>
		/// <returns></returns>
		public static IApplicationBuilder UseSwaggerVersioned( this IApplicationBuilder app ) {
			app.UseSwagger( );
			app.UseSwaggerUI( options => {
				var versionProvider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>( );

				foreach ( var versionDescription in versionProvider.ApiVersionDescriptions ) {
					options.SwaggerEndpoint(
						$"/swagger/{versionDescription.GroupName}/swagger.json",
						versionDescription.GroupName.ToUpperInvariant( ) );
				}
			} );

			return app;
		}
	}
}