using Myth.Interfaces;
using Myth.Models;

namespace Myth.Flow.Actions.Test.Models;

public class FailingQueryHandler : IQueryHandler<FailingQuery, string> {

	public Task<QueryResult<string>> HandleAsync(
		FailingQuery query,
		CancellationToken cancellationToken = default ) {
		QueryResult<string> result = query.FailureMode switch {
			"forbidden" => QueryResult<string>.Forbidden( ),
			"not-found" => QueryResult<string>.NotFound( "Resource not found" ),
			"unauthorized" => QueryResult<string>.Unauthorized( ),
			"no-content" => QueryResult<string>.NoContent( ),
			_ => QueryResult<string>.Failure( "Generic failure" )
		};

		return Task.FromResult( result );
	}
}
