using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Myth.Builders;
using Myth.Models;

namespace Myth.Extensions;

/// <summary>
/// Provides extension methods for registering Myth.Flow services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions {

	/// <summary>
	/// Adds Myth.Flow services to the dependency injection container with default configuration.
	/// Registers <see cref="PipelineConfiguration"/>, telemetry, and automatically initializes
	/// the global service provider for seamless pipeline creation.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
	/// <returns>The updated <see cref="IServiceCollection"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
	/// <remarks>
	/// This method automatically registers the global service provider using the Myth.Commons
	/// centralized service provider management. Uses default configuration with telemetry and logging enabled.
	/// </remarks>
	public static IServiceCollection AddFlow( this IServiceCollection services ) {
		ArgumentNullException.ThrowIfNull( services );

		var config = new PipelineConfiguration( );

		return RegisterFlowServices( services, config );
	}

	/// <summary>
	/// Adds Myth.Flow services to the dependency injection container using a fluent configuration builder.
	/// Registers <see cref="PipelineConfiguration"/>, telemetry, and automatically initializes
	/// the global service provider for seamless pipeline creation.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
	/// <param name="configure">Action to configure Myth.Flow using the fluent builder.</param>
	/// <returns>The updated <see cref="IServiceCollection"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="configure"/> is null.</exception>
	/// <remarks>
	/// This method automatically registers the global service provider using the Myth.Commons
	/// centralized service provider management. No additional configuration is required.
	/// </remarks>
	/// <example>
	/// <code>
	/// services.AddFlow( builder => builder
	///     .UseTelemetry( )
	///     .UseLogging( )
	///     .UseRetry( 3, 100 )
	///     .UseActivitySource( "MyApp.Pipeline" ) );
	/// </code>
	/// </example>
	public static IServiceCollection AddFlow(
		this IServiceCollection services,
		Action<FlowConfigurationBuilder> configure ) {
		ArgumentNullException.ThrowIfNull( services );
		ArgumentNullException.ThrowIfNull( configure );

		var builder = new FlowConfigurationBuilder( );
		configure( builder );
		var config = builder.Build( );

		return RegisterFlowServices( services, config );
	}

	/// <summary>
	/// Registers the core Myth.Flow services with the provided configuration.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
	/// <param name="config">The <see cref="PipelineConfiguration"/> to use.</param>
	/// <returns>The updated <see cref="IServiceCollection"/>.</returns>
	private static IServiceCollection RegisterFlowServices( IServiceCollection services, PipelineConfiguration config ) {
		// Register configuration
		services.AddSingleton( config );

		// Register ActivitySource for telemetry - SEMPRE registra se telemetria está habilitada
		if ( config.EnableTelemetry ) {
			services.AddSingleton( sp =>
				config.ActivitySource ?? new ActivitySource( "Myth.Flow" ) );
		}

		return services;
	}
}
