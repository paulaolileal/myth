namespace Myth.Flow.Interfaces;

/// <summary>
/// Handles command execution
/// </summary>
/// <typeparam name="TCommand">Command type</typeparam>
public interface ICommandHandler<in TCommand>
	where TCommand : ICommand {

	/// <summary>
	/// Handles the command
	/// </summary>
	Task<CommandResult> HandleAsync( TCommand command, CancellationToken cancellationToken = default );
}

/// <summary>
/// Handles command execution with typed response
/// </summary>
/// <typeparam name="TCommand">Command type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public interface ICommandHandler<in TCommand, TResponse>
	where TCommand : ICommand<TResponse> {

	/// <summary>
	/// Handles the command
	/// </summary>
	Task<CommandResult<TResponse>> HandleAsync( TCommand command, CancellationToken cancellationToken = default );
}