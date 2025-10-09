namespace Myth.Models;

/// <summary>
/// Represents a message in the dead letter queue
/// </summary>
public sealed class DeadLetterMessage {
	public object Message { get; init; }
	public Exception Exception { get; init; }
	public EventMetadata? Metadata { get; init; }
	public DateTimeOffset EnqueuedAt { get; init; }
}