using Microsoft.Extensions.DependencyInjection;
using Myth.Rest;

namespace Myth.DependencyInjection {

	public static class ServiceCollectionExtensions {

		/// <summary>
		/// Add to the service collection the dependency injection of Rest Content
		/// </summary>
		/// <param name="services">The collection of services</param>
		/// <param name="configurationBuilder">The default configuration</param>
		/// <param name="lifetime">The lifetime of the service</param>
		/// <returns>The service collection</returns>
		public static IServiceCollection AddRestContent( this IServiceCollection services, Action<ConfigurationBuilder>? configurationBuilder, ServiceLifetime lifetime = ServiceLifetime.Scoped ) {
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

		/// <summary>
		/// Add to the service collection the dependency injection of Rest File
		/// </summary>
		/// <param name="services">The collection of services</param>
		/// <param name="configurationBuilder">The default configuration</param>
		/// <param name="lifetime">The lifetime of the service</param>
		/// <returns>The service collection</returns>
		public static IServiceCollection AddRestFile( this IServiceCollection services, Action<ConfigurationBuilder>? configurationBuilder, ServiceLifetime lifetime = ServiceLifetime.Scoped ) {
			var serviceDescriptor = ServiceDescriptor.Describe(
				typeof( RestFileBuilder ),
				( serviceProvider ) =>
					Rest.Rest
						.File( )
						.Configure( configurationBuilder )
				,
				lifetime );

			services.Add( serviceDescriptor );

			return services;
		}
	}
}