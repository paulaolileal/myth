using Microsoft.Extensions.DependencyInjection;
using Myth.Exceptions;
using Myth.ValueProviders;

namespace Myth.Extensions;

public static class ServiceCollectionExtensions {

	public static IServiceCollection AddServiceFromType<TType>( this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped ) {
		var types = TypeProvider.GetTypesAssignableFrom<TType>( );

		foreach ( var type in types ) {
			var typeInterface = type
				.GetInterfaces( )
				.FirstOrDefault( x => x.Name.Contains( type.Name ) );

			if ( typeInterface is null )
				throw new InterfaceNotFoundException( $"Not found a interface that corresponds to type `{type.Namespace}.{type.Name}`" );

			var descriptor = new ServiceDescriptor( typeInterface, type, serviceLifetime );
			services.Add( descriptor );
		}

		return services;
	}
}