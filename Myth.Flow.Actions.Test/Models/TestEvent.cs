using Myth.Models;

namespace Myth.Flow.Actions.Test.Models;

public record TestEvent : DomainEvent {
	public required string Message { get; init; }
}
