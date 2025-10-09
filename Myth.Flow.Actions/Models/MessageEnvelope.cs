namespace Myth.Models;

/// <summary>
/// Envelope for message transport
/// </summary>
public sealed class MessageEnvelope {
	public string MessageId { get; init; }
	public string MessageType { get; init; }
	public string Payload { get; init; }
	public Dictionary<string, string> Headers { get; init; }
	public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
	public string? CorrelationId { get; init; }
	public string? CausationId { get; init; }
}