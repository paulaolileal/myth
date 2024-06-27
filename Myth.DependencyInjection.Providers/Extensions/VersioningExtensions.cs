using Asp.Versioning;
using Microsoft.Extensions.DependencyInjection;

namespace Myth.Extensions;

public static class VersioningExtensions {

	public static IServiceCollection AddVersioning( this IServiceCollection services, double currentVersion ) {
		var versioningService = services.AddApiVersioning( options => {
			options.ReportApiVersions = true;
			options.DefaultApiVersion = new ApiVersion( currentVersion );
			options.AssumeDefaultVersionWhenUnspecified = true;
			options.ApiVersionReader = ApiVersionReader.Combine(
				new UrlSegmentApiVersionReader( ),
				new HeaderApiVersionReader( "X-API-Version" ),
				new MediaTypeApiVersionReader( "X-API-Version" ) );
		} );

		versioningService.AddApiExplorer( options => {
			options.GroupNameFormat = "'v'VVV";
			options.SubstituteApiVersionInUrl = true;
		} );

		return services;
	}
}