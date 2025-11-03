using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Myth.Interfaces.Results;
using Myth.Models.Results;
using Myth.ValueProviders;

namespace Myth.Extensions.Mapping;

public static class AutoMapperExtensions {

	/// <summary>
	/// Makes available AutoMapper with pre-built configurations
	/// </summary>
	/// <param name="services">The service collection</param>
	/// <param name="options">Customizations for mapping</param>
	/// <returns>The service collection</returns>
	/// <remarks>
	/// The pagination types will be pre-builted mapped
	/// </remarks>
	public static IServiceCollection AddTypeMapping( this IServiceCollection services, Action<IMapperConfigurationExpression>? options = null ) {
		var assemblies = TypeProvider.ApplicationAssemblies;

		services.AddAutoMapper( conf => {
			conf.CreateMap( typeof( IPaginated<> ), typeof( Paginated<> ) );
			conf.CreateMap( typeof( IPaginated<> ), typeof( IPaginated<> ) ).As( typeof( Paginated<> ) );

			options?.Invoke( conf );
		}, assemblies );

		var serviceProvider = services.BuildServiceProvider( );
		var mapper = serviceProvider.GetRequiredService<IMapper>( );

		TypeMappingProvider.Configure( mapper );

		return services;
	}
}