using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Myth.Builders;
using Myth.Flow.Actions.Brokers;
using Myth.Flow.Actions.Hosting;
using Myth.Flow.Actions.Services;
using Myth.Flow.Actions.Settings;
using Myth.Flow.Resilience;
using Myth.Interfaces;
using Myth.Models;
using System.Diagnostics;

namespace Myth.Flow.Actions.Extensions;

/// <summary>
/// Dependency injection extensions for Flow.Actions
/// </summary>
public static class ServiceCollectionExtensions {

	/// <summary>
	/// Adds Flow.Actions services to the service collection with CQRS, event bus, and message broker support.
	/// Automatically initializes the global service provider using the centralized Myth system.
	/// </summary>
	/// <param name="services">The service collection to add services to</param>
	/// <param name="configure">Configuration action for Flow.Actions builder</param>
	/// <returns>The service collection for method chaining</returns>
	/// <exception cref="ArgumentNullException">Thrown when services or configure is null</exception>
	[Obsolete( "Use AddFlow with FlowWithActionsBuilder for unified configuration of Flow and Flow.Actions." )]
	public static IServiceCollection AddFlowActions(
		this IServiceCollection services,
		Action<FlowActionsBuilder> configure ) {
		ArgumentNullException.ThrowIfNull( services );
		ArgumentNullException.ThrowIfNull( configure );

		var builder = new FlowActionsBuilder( );
		configure( builder );
		var configuration = builder.Build( );

		services.AddSingleton( configuration );

		RegisterCore( services, configuration );
		RegisterMessageBroker( services, configuration );
		RegisterCache( services, configuration );
		RegisterTelemetry( services, configuration );
		RegisterHandlers( services, configuration );

		services.AddHostedService<MessageBrokerHostedService>( );

		return services;
	}


	/// <summary>
	/// Adds both Myth.Flow and Myth.Flow.Actions services using a unified configuration builder.
	/// This provides a seamless API where Flow.Actions is configured as an extension of Flow.
	/// </summary>
	/// <param name="services">The service collection to add services to</param>
	/// <param name="configure">Configuration action using the unified builder</param>
	/// <returns>The service collection for method chaining</returns>
	/// <exception cref="ArgumentNullException">Thrown when services or configure is null</exception>
	/// <example>
	/// <code>
	/// services.AddFlow( config => config
	///     .UseLogging( )
	///     .UseTelemetry( )
	///     .UseRetry( 3 )
	///     .UseActions( actions => actions
	///         .UseInMemory( )
	///         .ScanAssemblies( typeof( MyHandler ).Assembly )));
	/// </code>
	/// </example>
	public static IServiceCollection AddFlow(
		this IServiceCollection services,
		Action<FlowWithActionsBuilder> configure ) {
		ArgumentNullException.ThrowIfNull( services );
		ArgumentNullException.ThrowIfNull( configure );

		var builder = new FlowWithActionsBuilder( );
		configure( builder );

		// Build both configurations
		var flowConfig = builder.BuildFlowConfiguration( );
		var actionsConfig = builder.BuildActionsConfiguration( );

		// Register Flow services first
		RegisterFlowServices( services, flowConfig );

		// Register Flow.Actions services
		services.AddSingleton( actionsConfig );
		RegisterCore( services, actionsConfig );
		RegisterMessageBroker( services, actionsConfig );
		RegisterCache( services, actionsConfig );
		RegisterTelemetry( services, actionsConfig );
		RegisterHandlers( services, actionsConfig );
		services.AddHostedService<MessageBrokerHostedService>( );

		return services;
	}

	/// <summary>
	/// Registers the core Myth.Flow services with the provided configuration
	/// </summary>
	/// <param name="services">The service collection to add services to</param>
	/// <param name="config">The PipelineConfiguration to use</param>
	/// <returns>The updated service collection</returns>
	private static IServiceCollection RegisterFlowServices( IServiceCollection services, Myth.Models.PipelineConfiguration config ) {
		// Register configuration
		services.AddSingleton( config );

		// Register ActivitySource for telemetry
		if ( config.EnableTelemetry ) {
			services.AddSingleton( sp =>
				config.ActivitySource ?? new System.Diagnostics.ActivitySource( "Myth.Flow" ) );
		}

		return services;
	}

	/// <summary>
	/// Registers core Flow.Actions services including dispatcher and event bus
	/// </summary>
	/// <param name="services">The service collection</param>
	/// <param name="configuration">The Flow.Actions configuration</param>
	private static void RegisterCore( IServiceCollection services, FlowActionsConfiguration configuration ) {
		services.TryAddSingleton<IDispatcher, Dispatcher>( );
		services.TryAddSingleton<IEventBus, EventBus>( );
		services.TryAddSingleton<IEventSubscriptionManager, EventSubscriptionManager>( );
	}

	/// <summary>
	/// Registers the configured message broker (InMemory, Kafka, or RabbitMQ)
	/// </summary>
	/// <param name="services">The service collection</param>
	/// <param name="configuration">The Flow.Actions configuration</param>
	private static void RegisterMessageBroker( IServiceCollection services, FlowActionsConfiguration configuration ) {
		switch ( configuration.BrokerType ) {
			case MessageBrokerType.InMemory:
			var inMemoryOptions = new InMemoryBrokerOptions( );
			if ( configuration.BrokerConfigurationFactory != null ) {
				var options = configuration.BrokerConfigurationFactory( );
				if ( options is InMemoryBrokerOptions opt )
					inMemoryOptions = opt;
			}

			services.AddSingleton( inMemoryOptions );

			if ( inMemoryOptions.EnableDeadLetterQueue ) {
				services.AddSingleton<DeadLetterQueue>( sp => {
					var logger = sp.GetRequiredService<ILogger<DeadLetterQueue>>( );
					return new DeadLetterQueue( logger, maxSize: 10000 );
				} );
			}

			services.AddSingleton<IMessageBroker>( sp => {
				var subscriptionManager = sp.GetRequiredService<IEventSubscriptionManager>( );
				var logger = sp.GetRequiredService<ILogger<InMemoryBroker>>( );
				var activitySource = sp.GetService<ActivitySource>( ) ?? new ActivitySource( "Myth.Flow.Actions" );
				var dlq = inMemoryOptions.EnableDeadLetterQueue ? sp.GetService<DeadLetterQueue>( ) : null;

				return new InMemoryBroker(
					subscriptionManager,
					logger,
					activitySource,
					inMemoryOptions,
					dlq );
			} );
			break;

			case MessageBrokerType.Kafka:
			var kafkaOptions = new KafkaOptions {
				BootstrapServers = "localhost:9092",
				GroupId = "flow-actions"
			};

			if ( configuration.BrokerConfigurationFactory != null ) {
				var options = configuration.BrokerConfigurationFactory( );
				if ( options is KafkaOptions opt )
					kafkaOptions = opt;
			}

			services.AddSingleton( kafkaOptions );
			services.AddSingleton<IMessageBroker>( sp => {
				var logger = sp.GetRequiredService<ILogger<KafkaBroker>>( );
				return new KafkaBroker( kafkaOptions, logger );
			} );
			break;

			case MessageBrokerType.RabbitMQ:
			var rabbitOptions = new RabbitMQOptions {
				HostName = "localhost",
				UserName = "guest",
				Password = "guest"
			};

			if ( configuration.BrokerConfigurationFactory != null ) {
				var options = configuration.BrokerConfigurationFactory( );
				if ( options is RabbitMQOptions opt )
					rabbitOptions = opt;
			}

			services.AddSingleton( rabbitOptions );
			services.AddSingleton<IMessageBroker>( sp => {
				var logger = sp.GetRequiredService<ILogger<RabbitMQBroker>>( );
				return new RabbitMQBroker( rabbitOptions, logger );
			} );
			break;
		}
	}

	/// <summary>
	/// Registers caching services if caching is enabled (Memory or Distributed Redis)
	/// </summary>
	/// <param name="services">The service collection</param>
	/// <param name="configuration">The Flow.Actions configuration</param>
	private static void RegisterCache( IServiceCollection services, FlowActionsConfiguration configuration ) {
		if ( !configuration.CachingEnabled )
			return;

		var cacheConfig = new CacheConfiguration( );
		configuration.CacheConfiguration?.Invoke( cacheConfig );

		services.AddSingleton( cacheConfig );

		switch ( cacheConfig.ProviderType ) {
			case CacheProviderType.Memory:
			services.AddMemoryCache( );
			services.AddSingleton<ICacheProvider>( sp => {
				var cache = sp.GetRequiredService<IMemoryCache>( );
				var logger = sp.GetRequiredService<ILogger<MemoryCacheProvider>>( );
				return new MemoryCacheProvider( cache, logger );
			} );
			break;

			case CacheProviderType.Distributed:
			if ( !string.IsNullOrEmpty( cacheConfig.ConnectionString ) ) {
				services.AddStackExchangeRedisCache( options => {
					options.Configuration = cacheConfig.ConnectionString;
				} );
			}
			break;
		}
	}

	/// <summary>
	/// Registers OpenTelemetry ActivitySource for distributed tracing if telemetry is enabled
	/// </summary>
	/// <param name="services">The service collection</param>
	/// <param name="configuration">The Flow.Actions configuration</param>
	private static void RegisterTelemetry( IServiceCollection services, FlowActionsConfiguration configuration ) {
		if ( configuration.TelemetryEnabled )
			services.TryAddSingleton( new ActivitySource( "Myth.Flow.Actions" ) );
	}

	/// <summary>
	/// Scans assemblies and registers all command, query, and event handlers
	/// </summary>
	/// <param name="services">The service collection</param>
	/// <param name="configuration">The Flow.Actions configuration</param>
	private static void RegisterHandlers( IServiceCollection services, FlowActionsConfiguration configuration ) {
		if ( !configuration.AssembliesToScan.Any( ) )
			return;

		var scanner = new AssemblyScanner( );
		var handlerTypes = scanner.ScanForHandlers( configuration.AssembliesToScan.ToArray( ) );

		foreach ( var (interfaceType, implementationType) in handlerTypes )
			services.AddTransient( interfaceType, implementationType );

		var eventHandlers = scanner.ScanForEventHandlers( configuration.AssembliesToScan.ToArray( ) );

		foreach ( var (eventType, handlerType) in eventHandlers ) {
			var eventHandlerInterface = typeof( IEventHandler<> ).MakeGenericType( eventType );

			// Register both the interface and the concrete type
			services.AddTransient( eventHandlerInterface, handlerType );
			services.AddTransient( handlerType );
		}

		services.AddSingleton<IEventHandlerRegistry>( sp => {
			var subscriptionManager = sp.GetRequiredService<IEventSubscriptionManager>( );
			var registry = new EventHandlerRegistry( subscriptionManager );

			foreach ( var (eventType, handlerType) in eventHandlers )
				registry.RegisterHandler( eventType, handlerType );

			return registry;
		} );

		// Register auto-subscription service if enabled
		if ( configuration.AutoSubscribeEventHandlers ) {
			services.AddSingleton( configuration );
			services.AddHostedService<EventHandlerAutoSubscriptionService>( );
		}
	}
}