using Myth.Interfaces;

namespace Myth.Flow.Actions.Test.Models;

public record TestCommand : ICommand<string> {
	public required string Value { get; init; }
}
