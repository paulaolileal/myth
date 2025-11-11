using Myth.Interfaces.Results;
using Myth.Models.Results;
using Myth.ValueObjects;

namespace Myth.Extensions;

public static class EnumerableExtension {

	/// <summary>
	/// Paginates a collection of elements
	/// </summary>
	/// <typeparam name="TEntity">The type of entity</typeparam>
	/// <param name="items">The collection of elements</param>
	/// <param name="totalItems">The total of item in collection</param>
	/// <param name="take">The amount of items to take</param>
	/// <param name="skip">The amount of items to skip</param>
	/// <returns>The paginated object with elements</returns>
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

		var itemsProcessed = items
			.Skip( pageNumber )
			.Take( pageSize )
			.ToList( );

		var paginatedResult = new Paginated<TEntity>(
			pageNumber + 1,
			pageSize,
			totalItems,
			totalPages,
			itemsProcessed );

		return paginatedResult;
	}

	/// <summary>
	/// Paginates a collection of elements
	/// </summary>
	/// <typeparam name="TEntity">The type of entity</typeparam>
	/// <param name="items">The collection of elements</param>
	/// <param name="take">The amount of items to take</param>
	/// <param name="skip">The amount of items to skip</param>
	/// <returns>The paginated object with elements</returns>
	public static IPaginated<TEntity> AsPaginated<TEntity>( this IEnumerable<TEntity> items, int take = 0, int skip = 0 ) =>
		items.AsPaginated( items.Count( ), take, skip );

	/// <summary>
	/// Paginates a collection of elements
	/// </summary>
	/// <typeparam name="TEntity">The type of entity</typeparam>
	/// <param name="items">The collection of elements</param>
	/// <param name="pagination">The pagination object containing page number and page size</param>
	/// <returns>The paginated object with elements</returns>
	public static IPaginated<TEntity> AsPaginated<TEntity>( this IEnumerable<TEntity> items, Pagination pagination ) {
		var skip = 0;
		if ( pagination.PageNumber > 0 )
			skip = ( pagination.PageNumber - 1 ) * pagination.PageSize;

		return items.AsPaginated( pagination.PageSize, skip );
	}
}
