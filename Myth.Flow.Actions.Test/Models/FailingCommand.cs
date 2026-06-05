using Myth.Interfaces;

namespace Myth.Flow.Actions.Test.Models;

public record FailingCommand : ICommand<string> {
	public required string FailureMode { get; init; }
}
