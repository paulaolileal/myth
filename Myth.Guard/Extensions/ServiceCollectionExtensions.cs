using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Myth.Interfaces;
using Myth.Models;
using Myth.Validation;

namespace Myth.Extensions {

	/// <summary>
	/// Extension methods for service collection
	/// </summary>
	public static class ServiceCollectionExtensions {

		/// <summary>
		/// Adds Myth.Guard validation services with optional exception handling configuration.
		/// Automatically initializes the global service provider using the centralized Myth system.
		/// </summary>
		/// <param name="services">The service collection</param>
		/// <param name="configure">Optional action to configure exception handling mappings</param>
		public static IServiceCollection AddGuard( this IServiceCollection services, Action<GuardOptions>? configure = null ) {
			var options = new GuardOptions( );

			var serviceProvider = services.BuildServiceProvider( );
			var environment = serviceProvider.GetService<IWebHostEnvironment>( );
			options.Environment = environment;

			configure?.Invoke( options );

			services.AddSingleton( options );
			services.AddScoped<IValidator, Validator>( );

			return services;
		}
	}
}