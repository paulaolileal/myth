using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Myth.Extensions;
using Myth.Interfaces.Repositories.EntityFramework;
using Myth.Interfaces.Results;
using Myth.ValueObjects;

namespace Myth.Repositories.EntityFramework;

public partial class ReadRepositoryAsync<TEntity> : IReadRepositoryAsync<TEntity> where TEntity : class {

	/// <summary>
	/// Searches for all elements that are satisfied by filter predicate with optional ordering
	/// </summary>
	/// <param name="filterPredicate">Predicate for filtering entities</param>
	/// <param name="orderPredicate">Optional predicate for ordering entities</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>An enumerable collection of entities that satisfy the filter predicate</returns>
	public virtual async Task<IEnumerable<TEntity>> SearchAsync( Expression<Func<TEntity, bool>> filterPredicate, Expression<Func<TEntity, bool>>? orderPredicate = null, CancellationToken cancellationToken = default ) {
		var query = _context
			.Set<TEntity>( )
			.Where( filterPredicate )
			.AsQueryable( );

		if ( orderPredicate is not null )
			query = query.OrderBy( orderPredicate );

		var result = await query.ToListAsync( cancellationToken );

		return result.AsEnumerable( );
	}

	/// <summary>
	/// Searches for all elements that are satisfied by filter predicate and returns paginated results
	/// </summary>
	/// <param name="filterPredicate">Predicate for filtering entities</param>
	/// <param name="take">Number of items to take (0 for no limit)</param>
	/// <param name="skip">Number of items to skip (0 for no skip)</param>
	/// <param name="orderPredicate">Optional predicate for ordering entities</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A paginated object containing filtered entities</returns>
	public virtual async Task<IPaginated<TEntity>> SearchPaginatedAsync( Expression<Func<TEntity, bool>> filterPredicate, int take = 0, int skip = 0, Expression<Func<TEntity, bool>>? orderPredicate = null, CancellationToken cancellationToken = default ) {
		var query = _context
			.Set<TEntity>( )
			.Where( filterPredicate )
			.AsQueryable( );

		if ( orderPredicate is not null )
			query = query.OrderBy( orderPredicate );

		var totalItems = await _context
			.Set<TEntity>( )
			.Where( filterPredicate )
			.CountAsync( cancellationToken );

		if ( skip > 0 )
			query = query.Skip( skip );

		if ( take > 0 )
			query = query.Take( take );

		var items = await query.ToListAsync( cancellationToken );

		return items.AsPaginated( totalItems, take, skip );
	}

	/// <summary>
	/// Filters sequence of values based on predicate and returns as queryable
	/// </summary>
	/// <param name="predicate">Expression predicate to filter entities</param>
	/// <returns>A queryable collection that satisfies the predicate</returns>
	public virtual IQueryable<TEntity> Where( Expression<Func<TEntity, bool>> predicate ) =>
		_context
			.Set<TEntity>( )
			.Where( predicate )
			.AsQueryable( );

	/// <summary>
	/// Counts the number of elements that satisfy the predicate
	/// </summary>
	/// <param name="predicate">Expression predicate to filter entities</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The number of elements that satisfy the predicate</returns>
	public virtual Task<int> CountAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default ) =>
		_context
			.Set<TEntity>( )
			.CountAsync( predicate, cancellationToken );

	/// <summary>
	/// Determines whether any element in the sequence satisfies the predicate
	/// </summary>
	/// <param name="predicate">Expression predicate to test entities</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>True if any element satisfies the predicate; otherwise, false</returns>
	public virtual Task<bool> AnyAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default ) =>
		_context
			.Set<TEntity>( )
			.AnyAsync( predicate, cancellationToken );

	/// <summary>
	/// Determines whether all elements in the sequence satisfy the predicate
	/// </summary>
	/// <param name="predicate">Expression predicate to test entities</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>True if all elements satisfy the predicate; otherwise, false</returns>
	public virtual Task<bool> AllAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default ) =>
		_context
			.Set<TEntity>( )
			.AllAsync( predicate, cancellationToken );

	/// <summary>
	/// Returns the first element of the sequence that satisfies the predicate, or null if no element is found
	/// </summary>
	/// <param name="predicate">Expression predicate to filter entities</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The first entity that satisfies the predicate, or null if no entity is found</returns>
	public virtual Task<TEntity?> FirstOrDefaultAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default ) =>
		_context
			.Set<TEntity>( )
			.FirstOrDefaultAsync( predicate, cancellationToken );

	/// <summary>
	/// Returns the last element of the sequence that satisfies the predicate, or null if no element is found
	/// </summary>
	/// <param name="predicate">Expression predicate to filter entities</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The last entity that satisfies the predicate, or null if no entity is found</returns>
	public virtual Task<TEntity?> LastOrDefaultAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default ) =>
		_context
			.Set<TEntity>( )
			.LastOrDefaultAsync( predicate, cancellationToken );

	/// <summary>
	/// Returns the first element of the sequence that satisfies the predicate
	/// </summary>
	/// <param name="predicate">Expression predicate to filter entities</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The first entity that satisfies the predicate</returns>
	/// <exception cref="InvalidOperationException">Thrown when no element is found</exception>
	public virtual Task<TEntity> FirstAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default ) =>
		_context
			.Set<TEntity>( )
			.FirstAsync( predicate, cancellationToken );

	/// <summary>
	/// Returns the last element of the sequence that satisfies the predicate
	/// </summary>
	/// <param name="predicate">Expression predicate to filter entities</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The last entity that satisfies the predicate</returns>
	/// <exception cref="InvalidOperationException">Thrown when no element is found</exception>
	public virtual Task<TEntity> LastAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default ) =>
		_context
			.Set<TEntity>( )
			.LastAsync( predicate, cancellationToken );

	/// <summary>
	/// Searches for all elements that are satisfied by filter predicate and returns paginated results
	/// </summary>
	/// <param name="filterPredicate">Predicate for filtering entities</param>
	/// <param name="pagination">Pagination object with page number and page size</param>
	/// <param name="orderPredicate">Optional predicate for ordering entities</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A paginated object containing filtered entities</returns>
	public virtual async Task<IPaginated<TEntity>> SearchPaginatedAsync( Expression<Func<TEntity, bool>> filterPredicate, Pagination pagination, Expression<Func<TEntity, bool>>? orderPredicate = null, CancellationToken cancellationToken = default ) {
		var skip = pagination.PageNumber > 0 ? ( pagination.PageNumber - 1 ) * pagination.PageSize : 0;
		var take = pagination.PageSize;

		return await SearchPaginatedAsync( filterPredicate, take, skip, orderPredicate, cancellationToken );
	}
}
