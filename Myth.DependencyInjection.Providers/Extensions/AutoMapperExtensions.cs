using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Myth.Interfaces.Repositories.Results;
using Myth.Repositories.Results;
using Myth.ValueProviders;

namespace Myth.Extensions;

public static class AutoMapperExtensions {

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