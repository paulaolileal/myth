using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Myth.Builders;
using Myth.Flow.Actions.Brokers;
using Myth.Flow.Actions.Settings;
using Myth.Interfaces;
using Myth.Models;
using System.Diagnostics;

namespace Myth.Flow.Actions.Extensions;

/// <summary>
/// Dependency injection extensions for Flow.Actions
/// </summary>
public static class ServiceCollectionExtensions {

	/// <summary>
	/// Adds Flow.Actions services to the service collection
	/// </summary>
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

		return services;
	}

	private static void RegisterCore( IServiceCollection services, FlowActionsConfiguration configuration ) {
		services.TryAddSingleton<IDispatcher, Dispatcher>( );
		services.TryAddSingleton<IEventBus, EventBus>( );
		services.TryAddSingleton<IEventSubscriptionManager, EventSubscriptionManager>( );
	}

	private static void RegisterMessageBroker( IServiceCollection services, FlowActionsConfiguration configuration ) {
		switch ( configuration.BrokerType ) {
			case MessageBrokerType.InMemory:
			services.AddSingleton<IMessageBroker>( sp => {
				var logger = sp.GetRequiredService<ILogger<InMemoryBroker>>( );
				return new InMemoryBroker( logger );
			} );
			break;

			case MessageBrokerType.Kafka:
			var kafkaOptions = new KafkaOptions {
				BootstrapServers = "localhost:9092",
				GroupId = "flow-actions"
			};
			configuration.BrokerConfiguration?.Invoke( kafkaOptions );

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
			configuration.BrokerConfiguration?.Invoke( rabbitOptions );

			services.AddSingleton( rabbitOptions );
			services.AddSingleton<IMessageBroker>( sp => {
				var logger = sp.GetRequiredService<ILogger<RabbitMQBroker>>( );
				return new RabbitMQBroker( rabbitOptions, logger );
			} );
			break;
		}
	}

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

	private static void RegisterTelemetry( IServiceCollection services, FlowActionsConfiguration configuration ) {
		if ( configuration.TelemetryEnabled )
			services.TryAddSingleton( new ActivitySource( "Myth.Flow.Actions" ) );
	}

	private static void RegisterHandlers( IServiceCollection services, FlowActionsConfiguration configuration ) {
		if ( !configuration.AssembliesToScan.Any( ) )
			return;

		var scanner = new AssemblyScanner( );
		var handlerTypes = scanner.ScanForHandlers( configuration.AssembliesToScan.ToArray( ) );

		foreach ( var (interfaceType, implementationType) in handlerTypes )
			services.AddTransient( interfaceType, implementationType );

		var eventHandlers = scanner.ScanForEventHandlers( configuration.AssembliesToScan.ToArray( ) );

		services.AddSingleton<IEventHandlerRegistry>( sp => {
			var subscriptionManager = sp.GetRequiredService<IEventSubscriptionManager>( );
			var registry = new EventHandlerRegistry( subscriptionManager );

			foreach ( var (eventType, handlerType) in eventHandlers )
				registry.RegisterHandler( eventType, handlerType );

			return registry;
		} );
	}
}