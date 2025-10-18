using Myth.Interfaces;
using Myth.Models;

namespace Myth.Flow.Actions.Test.Models {

	public class TestQueryHandler : IQueryHandler<TestQuery, string> {

		public Task<QueryResult<string>> HandleAsync(
			TestQuery query,
			CancellationToken cancellationToken = default ) {
			var result = $"Result for: {query.Key}";
			return Task.FromResult( QueryResult<string>.Success( result ) );
		}
	}
}