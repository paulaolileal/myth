using Myth.Interfaces;

namespace Myth.Flow.Actions.Test.Models {
	public record TestCommandNoResponse : ICommand {
		public required string Value { get; init; }
	}
}