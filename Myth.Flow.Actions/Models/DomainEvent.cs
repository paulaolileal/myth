using Myth.Interfaces;

namespace Myth.Models;

/// <summary>
/// Base implementation for domain events
/// </summary>
public abstract record DomainEvent : IEvent {
	/// <summary>
	/// Unique identifier for the event. Automatically generated using a new GUID
	/// </summary>
	public string EventId { get; init; } = Guid.NewGuid( ).ToString( );

	/// <summary>
	/// Timestamp when the event occurred. Automatically set to current UTC time
	/// </summary>
	public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
