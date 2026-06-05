using Myth.Interfaces;

namespace Myth.Flow.Actions.Test.Models;

public record FailingQuery : IQuery<string> {
	public required string FailureMode { get; init; }
}
