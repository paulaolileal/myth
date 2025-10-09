namespace Myth.Interfaces;

/// <summary>
/// Message broker abstraction for publishing events
/// </summary>
public interface IMessageBroker {

	/// <summary>
	/// Publishes event to message broker
	/// </summary>
	Task PublishAsync<TEvent>( TEvent @event, CancellationToken cancellationToken = default )
		where TEvent : IEvent;

	/// <summary>
	/// Starts consuming messages
	/// </summary>
	Task StartAsync( CancellationToken cancellationToken = default );

	/// <summary>
	/// Stops consuming messages
	/// </summary>
	Task StopAsync( CancellationToken cancellationToken = default );
}