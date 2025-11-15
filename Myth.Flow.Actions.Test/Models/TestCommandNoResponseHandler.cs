using Myth.Interfaces;
using Myth.Models;

namespace Myth.Flow.Actions.Test.Models;

public class TestCommandNoResponseHandler : ICommandHandler<TestCommandNoResponse> {

	public Task<CommandResult> HandleAsync(
		TestCommandNoResponse command,
		CancellationToken cancellationToken = default ) {
		return Task.FromResult( CommandResult.Success( ) );
	}
}
