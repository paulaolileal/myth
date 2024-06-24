using Myth.Interfaces.Repositories.Results;

namespace Myth.Repositories.Results {

	public class Paginated<TEntity> : IPaginated<TEntity> {
		public int PageNumber { get; private set; }

		public int PageSize { get; private set; }

		public int TotalItems { get; private set; }

		public int TotalPages { get; private set; }

		public IEnumerable<TEntity> Items { get; private set; } = [ ];

		public Paginated(
			int pageNumber,
			int pageSize,
			int totalItems,
			int totalPages,
			IEnumerable<TEntity> items ) {
			PageNumber = pageNumber;
			PageSize = pageSize;
			TotalItems = totalItems;
			TotalPages = totalPages;
			Items = items;
		}
	}
}