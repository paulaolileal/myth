namespace Myth.Models;

/// <summary>
/// Metadata for event processing
/// </summary>
public sealed class EventMetadata {
	public string EventId { get; init; }
	public string EventType { get; init; }
	public DateTimeOffset OccurredAt { get; init; }
	public DateTimeOffset ProcessedAt { get; init; }
	public string? Source { get; init; }
	public string? CorrelationId { get; init; }
	public int RetryCount { get; set; }
	public Dictionary<string, object> Properties { get; init; } = new( );
}