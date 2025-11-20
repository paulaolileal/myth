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
		var pageSize = take > 0 ? take : totalItems;

		var pageNumber = pageSize > 0 ? ( skip / pageSize ) + 1 : 1;

		var totalPages = pageSize > 0 && totalItems > 0
			? ( int )Math.Ceiling( ( decimal )totalItems / pageSize )
			: totalItems > 0 ? 1 : 0;

		var query = items.AsQueryable( );

		if ( skip > 0 )
			query = query.Skip( skip );

		if ( take > 0 )
			query = query.Take( take );

		var itemsList = query.ToList( );

		var paginatedResult = new Paginated<TEntity>(
			pageNumber,
			pageSize,
			totalItems,
			totalPages,
			itemsList );

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

		return items.AsPaginated( items.Count( ), pagination.PageSize, skip );
	}
}
