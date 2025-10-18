namespace Myth.Models;

/// <summary>
/// Represents a message in the dead letter queue
/// </summary>
public sealed class DeadLetterMessage {

	/// <summary>
	/// The message that failed to process
	/// </summary>
	public object Message { get; init; }

	/// <summary>
	/// The exception that caused the message processing to fail
	/// </summary>
	public Exception Exception { get; init; }

	/// <summary>
	/// Optional metadata associated with the failed event
	/// </summary>
	public EventMetadata? Metadata { get; init; }

	/// <summary>
	/// Timestamp when the message was added to the dead letter queue
	/// </summary>
	public DateTimeOffset EnqueuedAt { get; init; }
}