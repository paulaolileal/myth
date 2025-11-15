namespace Myth.Models;

/// <summary>
/// Envelope for message transport
/// </summary>
public sealed class MessageEnvelope {

	/// <summary>
	/// Unique identifier for the message
	/// </summary>
	public string MessageId { get; init; }

	/// <summary>
	/// The fully qualified type name of the message
	/// </summary>
	public string MessageType { get; init; }

	/// <summary>
	/// Serialized message content
	/// </summary>
	public string Payload { get; init; }

	/// <summary>
	/// Additional headers for message routing and metadata
	/// </summary>
	public Dictionary<string, string> Headers { get; init; }

	/// <summary>
	/// Timestamp when the message envelope was created
	/// </summary>
	public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

	/// <summary>
	/// Correlation identifier for tracking related messages
	/// </summary>
	public string? CorrelationId { get; init; }

	/// <summary>
	/// Identifier of the message that caused this message to be created
	/// </summary>
	public string? CausationId { get; init; }
}
