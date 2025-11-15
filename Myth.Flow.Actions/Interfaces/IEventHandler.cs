namespace Myth.Interfaces;

/// <summary>
/// Handles event processing
/// </summary>
/// <typeparam name="TEvent">Event type</typeparam>
public interface IEventHandler<in TEvent>
	where TEvent : IEvent {

	/// <summary>
	/// Handles the event
	/// </summary>
	Task HandleAsync( TEvent @event, CancellationToken cancellationToken = default );
}
