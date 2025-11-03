namespace Myth.Models;

/// <summary>
/// Metadata for event processing
/// </summary>
public sealed class EventMetadata {

	/// <summary>
	/// Unique identifier for the event
	/// </summary>
	public string EventId { get; init; }

	/// <summary>
	/// The fully qualified type name of the event
	/// </summary>
	public string EventType { get; init; }

	/// <summary>
	/// Timestamp when the event occurred
	/// </summary>
	public DateTimeOffset OccurredAt { get; init; }

	/// <summary>
	/// Timestamp when the event was processed
	/// </summary>
	public DateTimeOffset ProcessedAt { get; init; }

	/// <summary>
	/// The source system or component that generated the event
	/// </summary>
	public string? Source { get; init; }

	/// <summary>
	/// Correlation identifier for tracking related events across distributed systems
	/// </summary>
	public string? CorrelationId { get; init; }

	/// <summary>
	/// Number of times this event has been retried
	/// </summary>
	public int RetryCount { get; set; }

	/// <summary>
	/// Additional custom properties associated with the event
	/// </summary>
	public Dictionary<string, object> Properties { get; init; } = new( );
}