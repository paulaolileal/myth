using Myth.Interfaces;
using Myth.Models;

namespace Myth.Flow.Actions.Test.Models {

	public class TestCommandHandler : ICommandHandler<TestCommand, string> {

		public Task<CommandResult<string>> HandleAsync(
			TestCommand command,
			CancellationToken cancellationToken = default ) {
			var result = $"Handled: {command.Value}";
			return Task.FromResult( CommandResult<string>.Success( result ) );
		}
	}
}