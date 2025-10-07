using Microsoft.Extensions.DependencyInjection;
using Myth.Builders;
using Myth.Flow;
using Myth.Interfaces;
using Myth.Models;
using System.Diagnostics;

namespace Myth.Extensions {

	/// <summary>
	/// Provides extension methods for registering Myth.Flow services in the dependency injection container.
	/// </summary>
	public static class ServiceCollectionExtensions {

		/// <summary>
		/// Adds Myth.Flow services to the dependency injection container.
		/// Registers <see cref="PipelineConfiguration"/>, telemetry, service provider accessor, and initializer.
		/// </summary>
		/// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
		/// <param name="configure">Optional action to configure <see cref="PipelineConfiguration"/>.</param>
		/// <returns>The updated <see cref="IServiceCollection"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
		/// <remarks>
		/// See <see cref="ServiceProviderAccessor"/>, <see cref="ServiceProviderInitializer"/>, and <see cref="Pipeline.SetGlobalServiceProvider"/> for related types and methods.
		/// </remarks>
		public static IServiceCollection AddFlow(
			this IServiceCollection services,
			Action<PipelineConfiguration>? configure = null ) {
			ArgumentNullException.ThrowIfNull( services );

			var config = new PipelineConfiguration( );
			configure?.Invoke( config );

			// Register configuration
			services.AddSingleton( config );

			// Register ActivitySource for telemetry - SEMPRE registra se telemetria está habilitada
			if ( config.EnableTelemetry ) {
				services.AddSingleton( sp =>
					config.ActivitySource ?? new ActivitySource( "Myth.Flow" ) );
			}

			// Register service provider accessor
			services.AddSingleton<IServiceProviderAccessor>( sp =>
				new ServiceProviderAccessor( sp ) );

			// Set global service provider after container is built
			services.AddSingleton<IServiceProviderInitializer>( sp => {
				Pipeline.SetGlobalServiceProvider( sp );
				return new ServiceProviderInitializer( sp );
			} );

			return services;
		}
	}
}