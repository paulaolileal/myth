using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Myth.Builders;
using Myth.Extensions;
using Myth.Interfaces;
using Myth.Rest;

namespace Myth.DependencyInjection
{

	public static class ServiceCollectionExtensions
	{

		/// <summary>
		/// Add to the service collection the dependency injection of REST builder.
		/// Automatically initializes the global service provider using the centralized Myth system.
		/// </summary>
		/// <param name="services">The collection of services</param>
		/// <param name="configurationBuilder">The default configuration</param>
		/// <param name="lifetime">The lifetime of the service</param>
		/// <returns>The service collection</returns>
		public static IServiceCollection AddRest(this IServiceCollection services, Action<ConfigurationBuilder> configurationBuilder, ServiceLifetime lifetime = ServiceLifetime.Scoped)
		{
			var serviceDescriptor = ServiceDescriptor.Describe(
				typeof(IRestRequest),
				(serviceProvider) =>
					Rest.Rest
						.Create()
						.Configure(configurationBuilder)
				,
				lifetime);

			services.Add(serviceDescriptor);

			// Auto-initialize global service provider using Myth.Commons centralized system
			services.AddMythAutoInitialization( "Myth.Rest" );

			return services;
		}

		/// <summary>
		/// Add a centralized REST factory to manage multiple REST configurations.
		/// Automatically initializes the global service provider using the centralized Myth system.
		/// </summary>
		/// <param name="services">The collection of services</param>
		/// <param name="lifetime">The lifetime of the factory service</param>
		/// <returns>The service collection</returns>
		public static IServiceCollection AddRestFactory(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
		{
			services.TryAdd(ServiceDescriptor.Describe(typeof(IRestFactory), typeof(RestFactory), lifetime));

			// Auto-initialize global service provider using Myth.Commons centralized system
			services.AddMythAutoInitialization( "Myth.Rest.Factory" );

			return services;
		}

		/// <summary>
		/// Add a named REST configuration to the factory
		/// </summary>
		/// <param name="services">The collection of services</param>
		/// <param name="name">Configuration name</param>
		/// <param name="configurationBuilder">The configuration builder</param>
		/// <returns>The service collection</returns>
		public static IServiceCollection AddRestConfiguration(this IServiceCollection services, string name, Action<ConfigurationBuilder> configurationBuilder)
		{
			services.Configure<RestFactorySettings>(options =>
			{
				options.Configurations[name] = configurationBuilder;
			});

			return services;
		}
	}

	/// <summary>
	/// Options for REST factory configurations
	/// </summary>
	public class RestFactorySettings
	{
		public Dictionary<string, Action<ConfigurationBuilder>> Configurations { get; set; } = new();
	}
}