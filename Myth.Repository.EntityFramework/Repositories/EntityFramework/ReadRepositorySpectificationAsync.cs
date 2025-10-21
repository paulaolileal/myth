using Microsoft.EntityFrameworkCore;
using Myth.Extensions;
using Myth.Interfaces;
using Myth.Interfaces.Repositories.EntityFramework;
using Myth.Interfaces.Repositories.Results;

namespace Myth.Repositories.EntityFramework;

public partial class ReadRepositoryAsync<TEntity> : IReadRepositoryAsync<TEntity> where TEntity : class {

	/// <summary>
	/// Searches for all elements that are satisfied by specification
	/// </summary>
	/// <param name="specification">Predicate based on specification</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>An enumerable collection</returns>
	public virtual async Task<IEnumerable<TEntity>> SearchAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default ) {
		var result = await _context
			.Set<TEntity>( )
			.AsQueryable( )
			.Specify( specification )
			.ToListAsync( cancellationToken );

		return result.AsEnumerable();
	}

	/// <summary>
	/// Searches for all elements that are satisfied by specification and paginate
	/// </summary>
	/// <param name="specification">Predicate based on specification</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A paginated object with collection</returns>
	public virtual async Task<IPaginated<TEntity>> SearchPaginatedAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default ) =>
		await Task.Run( ( ) => {
			var processedEntitySet = _context
				.Set<TEntity>( )
				.Where( specification );

			var totalItems = processedEntitySet.Count( );

			processedEntitySet = specification.Sorted( processedEntitySet.AsQueryable( ) );

			processedEntitySet = specification.Processed( processedEntitySet.AsQueryable( ) );

			var items = processedEntitySet.ToList( );

			return items.AsPaginated(
				totalItems,
				specification.ItemsTaked,
				specification.ItemsSkiped );
		}, cancellationToken );

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
	public virtual Task<int> CountAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default ) =>
		_context
			.Set<TEntity>( )
			.CountAsync( specification.Predicate, cancellationToken );

	/// <summary>
	/// Searches if any element is satisfied by specification
	/// </summary>
	/// <param name="specification">Predicate based on specification</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A value that represents the answer</returns>
	public virtual Task<bool> AnyAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default ) =>
		_context
			.Set<TEntity>( )
			.AnyAsync( specification.Predicate, cancellationToken );

	/// <summary>
	/// Get the first element of collection that is satisfied by specification
	/// </summary>
	/// <param name="specification">Predicate based on specification</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>An entity or null if not found</returns>
	public virtual Task<TEntity?> FirstOrDefaultAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default ) =>
		_context
			.Set<TEntity>( )
			.FirstOrDefaultAsync( specification.Predicate, cancellationToken );

	/// <summary>
	/// Get the last element of collection that is satisfied by specification
	/// </summary>
	/// <param name="specification">Predicate based on specification</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>An entity or null if not found</returns>
	public virtual Task<TEntity?> LastOrDefaultAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default ) =>
		_context
			.Set<TEntity>( )
			.LastOrDefaultAsync( specification.Predicate, cancellationToken );

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