using Myth.Interfaces;

namespace Myth.Flow.Actions.Test.Models {
	public record TestQuery : IQuery<string> {
		public required string Key { get; init; }
	}
}