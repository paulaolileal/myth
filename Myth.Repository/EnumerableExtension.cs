using Myth.Interfaces.Repositories.Results;
using Myth.Repositories.Results;
using Myth.ValueObjects;

namespace Myth.Extensions {

	public static class EnumerableExtension {

		public static IPaginated<TEntity> AsPaginated<TEntity>( this IEnumerable<TEntity> items, int totalItems, int take = 0, int skip = 0 ) {
			var pageSize = totalItems;
			if ( take > 0 )
				pageSize = take;

			int pageNumber = 0;
			if ( skip > 0 )
				pageNumber = skip / pageSize;

			int totalPages = 0;
			if ( totalItems > 0 )
				totalPages = ( int )Math.Ceiling( decimal.Divide( totalItems, pageSize ) );

			var itensProcessed = items
				.Skip( pageNumber )
				.Take( pageSize )
				.ToList( );

			var paginatedResult = new Paginated<TEntity>(
				pageNumber + 1,
				pageSize,
				totalItems,
				totalPages,
				itensProcessed );

			return paginatedResult;
		}

		public static IPaginated<TEntity> AsPaginated<TEntity>( this IEnumerable<TEntity> items, int take = 0, int skip = 0 ) {
			return items.AsPaginated( items.Count( ), take, skip );
		}

		public static IPaginated<TEntity> AsPaginated<TEntity>( this IEnumerable<TEntity> itens, Pagination pagination ) {
			var skip = 0;
			if ( pagination.PageNumber > 0 )
				skip = ( pagination.PageNumber - 1 ) * pagination.PageSize;

			return itens.AsPaginated( pagination.PageSize, skip );
		}
	}
}