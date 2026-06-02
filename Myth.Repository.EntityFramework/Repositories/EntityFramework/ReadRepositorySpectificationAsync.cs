using Microsoft.EntityFrameworkCore;
using Myth.Extensions;
using Myth.Interfaces;
using Myth.Interfaces.Repositories.EntityFramework;
using Myth.Interfaces.Results;

namespace Myth.Repositories.EntityFramework;

public partial class ReadRepositoryAsync<TEntity> : IReadRepositoryAsync<TEntity> where TEntity : class {

	/// <summary>
	/// Searches for all elements that are satisfied by specification. Entities are tracked by the
	/// EF Core change tracker — modifications persist on the next <c>SaveChangesAsync</c> call.
	/// </summary>
	/// <param name="specification">Predicate based on specification</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A materialized, change-tracked read-only list</returns>
	public virtual async Task<IReadOnlyList<TEntity>> SearchAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default ) {
		return await _context
			.Set<TEntity>( )
			.AsQueryable( )
			.Specify( specification )
			.ToListAsync( cancellationToken );
	}

	/// <summary>
	/// Searches for all elements that are satisfied by specification without EF Core change tracking.
	/// Use for read-only scenarios such as projections and reports.
	/// </summary>
	/// <param name="specification">Predicate based on specification</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A materialized, non-tracked read-only list</returns>
	public virtual async Task<IReadOnlyList<TEntity>> SearchAsNoTrackingAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default ) {
		return await _context
			.Set<TEntity>( )
			.AsNoTracking( )
			.AsQueryable( )
			.Specify( specification )
			.ToListAsync( cancellationToken );
	}

	/// <summary>
	/// Searches for all elements that are satisfied by specification and paginate
	/// </summary>
	/// <param name="specification">Predicate based on specification</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A paginated object with collection</returns>
	public virtual async Task<IPaginated<TEntity>> SearchPaginatedAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default ) {
		var baseQuery = _context
			.Set<TEntity>( )
			.AsQueryable( )
			.Where( specification.Predicate );

		var totalItems = await baseQuery.CountAsync( cancellationToken );

		var processedQuery = specification.Sorted( baseQuery );

		processedQuery = specification.Processed( processedQuery );

		var items = await processedQuery.ToListAsync( cancellationToken );

		return items.AsPaginated(
			totalItems,
			specification.ItemsTaked,
			specification.ItemsSkiped );
	}

	/// <summary>
	/// Filter sequence of values based on specification predicate
	/// </summary>
	/// <param name="specification">Predicate based on specification</param>
	/// <returns>A queryable collection that satisfies the predicate</returns>
	public virtual IQueryable<TEntity> Where( ISpec<TEntity> specification ) =>
		_context
			.Set<TEntity>( )
			.Where( specification.Predicate );

	/// <summary>
	/// Count elements that are satisfied by specification
	/// </summary>
	/// <param name="specification">Predicate based on specification</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A value that represents the count</returns>
	public virtual Task<int> CountAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default ) {
		var query = _context.Set<TEntity>( ).AsQueryable( );
		query = specification.Included( query );
		query = specification.Filtered( query );
		return query.CountAsync( cancellationToken );
	}

	/// <summary>
	/// Searches if any element is satisfied by specification
	/// </summary>
	/// <param name="specification">Predicate based on specification</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A value that represents the answer</returns>
	public virtual Task<bool> AnyAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default ) {
		var query = _context.Set<TEntity>( ).AsQueryable( );
		query = specification.Included( query );
		query = specification.Filtered( query );
		return query.AnyAsync( cancellationToken );
	}

	/// <summary>
	/// Get the first element of collection that is satisfied by specification
	/// </summary>
	/// <param name="specification">Predicate based on specification</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>An entity or null if not found</returns>
	public virtual Task<TEntity?> FirstOrDefaultAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default ) =>
		specification
			.Prepare( _context.Set<TEntity>( ).AsQueryable( ) )
			.FirstOrDefaultAsync( cancellationToken );

	/// <summary>
	/// Get the last element of collection that is satisfied by specification
	/// </summary>
	/// <param name="specification">Predicate based on specification</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>An entity or null if not found</returns>
	public virtual Task<TEntity?> LastOrDefaultAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default ) =>
		specification
			.Prepare( _context.Set<TEntity>( ).AsQueryable( ) )
			.LastOrDefaultAsync( cancellationToken );

	/// <summary>
	/// Get the first element of collection that is satisfied by specification
	/// </summary>
	/// <param name="specification">Predicate based on specification</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>An entity</returns>
	/// <exception cref="InvalidOperationException">Thrown when no element is found</exception>
	public virtual Task<TEntity> FirstAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default ) =>
		specification
			.Prepare( _context.Set<TEntity>( ).AsQueryable( ) )
			.FirstAsync( cancellationToken );

	/// <summary>
	/// Get the last element of collection that is satisfied by specification
	/// </summary>
	/// <param name="specification">Predicate based on specification</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>An entity</returns>
	/// <exception cref="InvalidOperationException">Thrown when no element is found</exception>
	public virtual Task<TEntity> LastAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default ) =>
		specification
			.Prepare( _context.Set<TEntity>( ).AsQueryable( ) )
			.LastAsync( cancellationToken );

	/// <summary>
	/// Searches if all elements are satisfied by specification
	/// </summary>
	/// <param name="specification">Predicate based on specification</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A value that represents the answer</returns>
	public virtual Task<bool> AllAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default ) =>
		_context
			.Set<TEntity>( )
			.AllAsync( specification.Predicate, cancellationToken );
}
