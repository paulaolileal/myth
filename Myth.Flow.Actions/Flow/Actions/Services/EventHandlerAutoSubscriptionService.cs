using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Myth.Flow.Actions.Settings;
using Myth.Interfaces;

namespace Myth.Flow.Actions.Services;

/// <summary>
/// Background service that automatically subscribes discovered event handlers to the EventBus
/// </summary>
internal sealed class EventHandlerAutoSubscriptionService : IHostedService {
	private readonly IServiceProvider _serviceProvider;
	private readonly FlowActionsConfiguration _configuration;
	private readonly ILogger<EventHandlerAutoSubscriptionService> _logger;

	public EventHandlerAutoSubscriptionService(
		IServiceProvider serviceProvider,
		FlowActionsConfiguration configuration,
		ILogger<EventHandlerAutoSubscriptionService> logger ) {
		_serviceProvider = serviceProvider;
		_configuration = configuration;
		_logger = logger;
	}

	/// <summary>
	/// Starts the service and performs auto-subscription of event handlers
	/// </summary>
	public Task StartAsync( CancellationToken cancellationToken ) {
		try {
			using var scope = _serviceProvider.CreateScope( );
			var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>( );
			var registry = scope.ServiceProvider.GetRequiredService<IEventHandlerRegistry>( );

			var registeredHandlers = registry.GetRegisteredHandlers( );
			var subscriptionCount = 0;

			foreach ( var (eventType, handlerType) in registeredHandlers ) {
				try {
					var subscribeMethod = typeof( IEventBus ).GetMethod( nameof( IEventBus.Subscribe ) );
					var genericSubscribeMethod = subscribeMethod!.MakeGenericMethod( eventType, handlerType );
					genericSubscribeMethod.Invoke( eventBus, null );

					subscriptionCount++;

					_logger.LogDebug(
						"Auto-subscribed event handler {HandlerType} to event {EventType}",
						handlerType.Name,
						eventType.Name );
				} catch ( Exception ex ) {
					_logger.LogError( ex,
						"Failed to auto-subscribe event handler {HandlerType} to event {EventType}",
						handlerType.Name,
						eventType.Name );
				}
			}

			_logger.LogInformation(
				"Auto-subscribed {SubscriptionCount} event handlers to EventBus",
				subscriptionCount );

			return Task.CompletedTask;
		} catch ( Exception ex ) {
			_logger.LogError( ex, "Failed to auto-subscribe event handlers" );
			throw;
		}
	}

	/// <summary>
	/// Stops the service (no-op)
	/// </summary>
	public Task StopAsync( CancellationToken cancellationToken ) => Task.CompletedTask;
}
