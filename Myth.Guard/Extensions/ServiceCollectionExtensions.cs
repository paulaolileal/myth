using Microsoft.Extensions.DependencyInjection;
using Myth.Interfaces;
using Myth.Validation;

namespace Myth.Extensions {

	/// <summary>
	/// Extension methods for service collection
	/// </summary>
	public static class ServiceCollectionExtensions {

		/// <summary>
		/// Adds Myth.Guard validation services
		/// </summary>
		public static IServiceCollection AddGuard( this IServiceCollection services ) {
			services.AddScoped<IValidator, Validator>( );

			return services;
		}
	}
}