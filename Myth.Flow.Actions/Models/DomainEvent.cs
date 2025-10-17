using Myth.Interfaces;

namespace Myth.Models;

/// <summary>
/// Base implementation for domain events
/// </summary>
public abstract record DomainEvent : IEvent {
	public string EventId { get; init; } = Guid.NewGuid( ).ToString( );
	public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}