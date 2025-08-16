using Microsoft.Extensions.DependencyInjection;
using Myth.Rest;

namespace Myth.DependencyInjection {

	public static class ServiceCollectionExtensions {

		/// <summary>
		/// Add to the service collection the dependency injection of REST builder
		/// </summary>
		/// <param name="services">The collection of services</param>
		/// <param name="configurationBuilder">The default configuration</param>
		/// <param name="lifetime">The lifetime of the service</param>
		/// <returns>The service collection</returns>
		public static IServiceCollection AddRest( this IServiceCollection services, Action<ConfigurationBuilder>? configurationBuilder = null, ServiceLifetime lifetime = ServiceLifetime.Scoped ) {
			var serviceDescriptor = ServiceDescriptor.Describe(
				typeof( RestBuilder ),
				( serviceProvider ) =>
					Rest.Rest
						.Create( )
						.Configure( configurationBuilder )
				,
				lifetime );

			services.Add( serviceDescriptor );

			return services;
		}
	}
}