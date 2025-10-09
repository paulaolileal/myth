using Myth.Models;

namespace Myth.Interfaces;

/// <summary>
/// Handles query execution
/// </summary>
/// <typeparam name="TQuery">Query type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public interface IQueryHandler<in TQuery, TResponse>
	where TQuery : IQuery<TResponse> {

	/// <summary>
	/// Handles the query
	/// </summary>
	Task<QueryResult<TResponse>> HandleAsync( TQuery query, CancellationToken cancellationToken = default );
}