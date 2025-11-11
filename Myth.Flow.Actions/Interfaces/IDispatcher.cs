using Myth.Models;

namespace Myth.Interfaces;

/// <summary>
/// Dispatches commands, queries and events to their handlers
/// </summary>
public interface IDispatcher {

	/// <summary>
	/// Dispatches a command
	/// </summary>
	Task<CommandResult> DispatchCommandAsync<TCommand>( TCommand command, CancellationToken cancellationToken = default )
		where TCommand : ICommand;

	/// <summary>
	/// Dispatches a command with response
	/// </summary>
	Task<CommandResult<TResponse>> DispatchCommandAsync<TCommand, TResponse>( TCommand command, CancellationToken cancellationToken = default )
		where TCommand : ICommand<TResponse>;

	/// <summary>
	/// Dispatches a query
	/// </summary>
	Task<QueryResult<TResponse>> DispatchQueryAsync<TQuery, TResponse>( TQuery query, CacheOptions? cacheOptions = null, CancellationToken cancellationToken = default )
		where TQuery : IQuery<TResponse>;

	/// <summary>
	/// Publishes an event
	/// </summary>
	Task PublishEventAsync<TEvent>( TEvent @event, CancellationToken cancellationToken = default )
		where TEvent : IEvent;
}
