using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Myth.Flow.Interfaces;
using System.Collections.Concurrent;
using System.Diagnostics;

/// <summary>
/// Default event bus implementation
/// </summary>
internal sealed class EventBus : IEventBus {
	private readonly IServiceProvider _serviceProvider;
	private readonly IMessageBroker _messageBroker;
	private readonly ILogger<EventBus> _logger;
	private readonly ActivitySource _activitySource;
	private readonly ConcurrentDictionary<Type, List<Type>> _subscriptions = new( );

	public EventBus(
		IServiceProvider serviceProvider,
		IMessageBroker messageBroker,
		ILogger<EventBus> logger,
		ActivitySource activitySource ) {
		_serviceProvider = serviceProvider;
		_messageBroker = messageBroker;
		_logger = logger;
		_activitySource = activitySource;
	}

	public async Task PublishAsync<TEvent>( TEvent @event, CancellationToken cancellationToken = default )
		where TEvent : IEvent {
		using var activity = _activitySource.StartActivity( $"EventBus.Publish.{typeof( TEvent ).Name}" );

		try {
			var eventType = typeof( TEvent );

			_logger.LogInformation( "Publishing event {EventType} with ID {EventId}",
				eventType.Name, @event.EventId );

			await _messageBroker.PublishAsync( @event, cancellationToken );

			if ( _subscriptions.TryGetValue( eventType, out var handlers ) ) {
				var tasks = new List<Task>( );

				foreach ( var handlerType in handlers ) {
					tasks.Add( InvokeHandlerAsync( @event, handlerType, cancellationToken ) );
				}

				await Task.WhenAll( tasks );
			}

			activity?.SetStatus( ActivityStatusCode.Ok );
		} catch ( Exception ex ) {
			activity?.SetStatus( ActivityStatusCode.Error, ex.Message );
			_logger.LogError( ex, "Error publishing event {EventType}", typeof( TEvent ).Name );
			throw;
		}
	}

	public void Subscribe<TEvent, THandler>( )
		where TEvent : IEvent
		where THandler : IEventHandler<TEvent> {
		var eventType = typeof( TEvent );
		var handlerType = typeof( THandler );

		_subscriptions.AddOrUpdate(
			eventType,
			_ => new List<Type> { handlerType },
			( _, handlers ) => {
				if ( !handlers.Contains( handlerType ) )
					handlers.Add( handlerType );
				return handlers;
			} );

		_logger.LogInformation( "Subscribed {HandlerType} to {EventType}",
			handlerType.Name, eventType.Name );
	}

	public void Unsubscribe<TEvent, THandler>( )
		where TEvent : IEvent
		where THandler : IEventHandler<TEvent> {
		var eventType = typeof( TEvent );
		var handlerType = typeof( THandler );

		if ( _subscriptions.TryGetValue( eventType, out var handlers ) ) {
			handlers.Remove( handlerType );
			_logger.LogInformation( "Unsubscribed {HandlerType} from {EventType}",
				handlerType.Name, eventType.Name );
		}
	}

	private async Task InvokeHandlerAsync<TEvent>(
		TEvent @event,
		Type handlerType,
		CancellationToken cancellationToken )
		where TEvent : IEvent {
		using var scope = _serviceProvider.CreateScope( );
		using var activity = _activitySource.StartActivity( $"EventHandler.{handlerType.Name}" );

		try {
			var handler = scope.ServiceProvider.GetRequiredService( handlerType ) as IEventHandler<TEvent>;

			if ( handler != null ) {
				await handler.HandleAsync( @event, cancellationToken );
				activity?.SetStatus( ActivityStatusCode.Ok );
			}
		} catch ( Exception ex ) {
			activity?.SetStatus( ActivityStatusCode.Error, ex.Message );
			_logger.LogError( ex, "Error invoking handler {HandlerType} for event {EventType}",
				handlerType.Name, typeof( TEvent ).Name );
		}
	}
}