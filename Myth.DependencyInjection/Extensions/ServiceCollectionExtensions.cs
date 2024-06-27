using Microsoft.Extensions.DependencyInjection;
using Myth.Exceptions;
using Myth.ValueProviders;

namespace Myth.Extensions;

public static class ServiceCollectionExtensions {

	/// <summary>
	/// Add to service collection all pairs of interface and type of searched type
	/// </summary>
	/// <typeparam name="TType">Searched type</typeparam>
	/// <param name="services">The collection of services</param>
	/// <param name="serviceLifetime">The scope of service</param>
	/// <returns>The collection of service</returns>
	/// <exception cref="InterfaceNotFoundException"></exception>
	/// <remarks>
	/// Example:
	/// <para>
	/// Searching: <c>IRepository</c>
	/// </para>
	/// <para>
	/// Find:
	/// <para>
	/// - <c>(IPersonRepository, PersonRepository)</c>
	/// <para>
	/// </para>
	/// - <c>(IAddressRepository, AddressRepository)</c>
	/// </para>
	/// </para>
	/// Result: Both pairs will be injected
	/// </remarks>
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