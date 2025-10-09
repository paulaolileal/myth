namespace Myth.Flow.Interfaces;

/// <summary>
/// Represents a domain event
/// </summary>
public interface IEvent {

	/// <summary>
	/// Event identifier
	/// </summary>
	string EventId { get; }

	/// <summary>
	/// When the event occurred
	/// </summary>
	DateTimeOffset OccurredAt { get; }
}