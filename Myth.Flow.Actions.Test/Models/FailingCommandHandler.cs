using Myth.Interfaces;
using Myth.Models;

namespace Myth.Flow.Actions.Test.Models;

public class FailingCommandHandler : ICommandHandler<FailingCommand, string> {

	public Task<CommandResult<string>> HandleAsync(
		FailingCommand command,
		CancellationToken cancellationToken = default ) {
		CommandResult<string> result = command.FailureMode switch {
			"forbidden"            => CommandResult<string>.Forbidden( ),
			"not-found"            => CommandResult<string>.NotFound( "Resource not found" ),
			"unauthorized"         => CommandResult<string>.Unauthorized( ),
			"payment-required"     => CommandResult<string>.PaymentRequired( "Insufficient credits" ),
			"conflict"             => CommandResult<string>.Conflict( "Duplicate entry" ),
			"unprocessable-entity" => CommandResult<string>.UnprocessableEntity( "Invalid data" ),
			"no-content"           => CommandResult<string>.NoContent( ),
			_                      => CommandResult<string>.Failure( "Generic failure" )
		};

		return Task.FromResult( result );
	}
}
