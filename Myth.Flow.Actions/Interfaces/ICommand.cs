using Myth.Models;

namespace Myth.Interfaces;

/// <summary>
/// Represents a command that performs an action
/// </summary>
public interface ICommand : IRequest<CommandResult> { }

/// <summary>
/// Represents a command with a typed response
/// </summary>
/// <typeparam name="TResponse">Response type</typeparam>
public interface ICommand<TResponse> : IRequest<CommandResult<TResponse>> { }