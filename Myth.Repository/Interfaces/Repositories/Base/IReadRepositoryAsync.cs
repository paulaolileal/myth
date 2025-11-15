using System.Linq.Expressions;
using Myth.Interfaces.Results;
using Myth.ValueObjects;

namespace Myth.Interfaces.Repositories.Base;

public interface IReadRepositoryAsync<TEntity> : IRepository, IAsyncDisposable {

	/// <summary>
	/// Filter sequence of values based on predicate
	/// </summary>
	/// <param name="specification">Predicate based on specification</param>
	/// <returns>An queryable collection that satisfy the predicate</returns>
	IQueryable<TEntity> Where( ISpec<TEntity> specification );

	/// <summary>
	/// Filter sequence of values based on predicate
	/// </summary>
	/// <param name="specification">Predicate</param>
	/// <returns>An queryable collection that satisfy the predicate</returns>
	IQueryable<TEntity> Where( Expression<Func<TEntity, bool>> predicate );

	/// <summary>
	/// Return the original collection as queryable
	/// </summary>
	/// <returns>The queryable collection</returns>
	IQueryable<TEntity> AsQueryable( );

	/// <summary>
	/// Return the original collection as enumerable
	/// </summary>
	/// <returns>The enumerable collection</returns>
	IEnumerable<TEntity> AsEnumerable( );

	/// <summary>
	/// Searchs for all elements that is satisfyed by predicate
	/// </summary>
	/// <param name="specification">Predicate based on specification</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A enumerable collection</returns>
	Task<IEnumerable<TEntity>> SearchAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default );

	/// <summary>
	/// Searchs for all elements that is satisfyed by predicate
	/// </summary>
	/// <param name="filterPredicate">Predicate for filtering</param>
	/// <param name="orderPredicate">Predicate for ordering</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A enumerable collection</returns>
	Task<IEnumerable<TEntity>> SearchAsync( Expression<Func<TEntity, bool>> filterPredicate, Expression<Func<TEntity, bool>>? orderPredicate = null, CancellationToken cancellationToken = default );

	/// <summary>
	/// Searchs for all elements that is satisfyed by predicate and paginate
	/// </summary>
	/// <param name="specification">Predicate based on specification</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A IPaginated object with collection</returns>
	Task<IPaginated<TEntity>> SearchPaginatedAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default );

	/// <summary>
	/// Searchs for all elements that is satisfyed by predicate and paginate
	/// </summary>
	/// <param name="filterPredicate">Predicate based on expression</param>
	/// <param name="orderPredicate">Predicate for ordering</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A IPaginated object with collection</returns>
	Task<IPaginated<TEntity>> SearchPaginatedAsync( Expression<Func<TEntity, bool>> filterPredicate, int take = 0, int skip = 0, Expression<Func<TEntity, bool>>? orderPredicate = null, CancellationToken cancellationToken = default );

	/// <summary>
	/// Count elements that is satisfyed by predicate
	/// </summary>
	/// <param name="specification">Predicated based on specification</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A value that represents the count</returns>
	Task<int> CountAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default );

	/// <summary>
	/// Count elements that is satisfyed by predicate
	/// </summary>
	/// <param name="predicate">Predicated based on specification</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A value that represents the count</returns>
	Task<int> CountAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );

	/// <summary>
	/// Searchs if any element is satisfyed by predicate
	/// </summary>
	/// <param name="specification">Predicated based on specification</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A value that represents answer</returns>
	Task<bool> AnyAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default );

	/// <summary>
	/// Searchs if any element is satisfyed by predicate
	/// </summary>
	/// <param name="predicate">Predicate</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A value that represents answer</returns>
	Task<bool> AnyAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );

	/// <summary>
	/// Get the first element of collection that is satisfyed by predicate
	/// </summary>
	/// <param name="specification">Predicate based on specification</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A entity</returns>
	Task<TEntity?> FirstOrDefaultAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default );

	/// <summary>
	/// Get the first element of collection that is satisfyed by predicate
	/// </summary>
	/// <param name="predicate">Predicate based on specification</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A entity</returns>
	Task<TEntity?> FirstOrDefaultAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );

	/// <summary>
	/// Get the last element of collection that is satisfyed by predicate
	/// </summary>
	/// <param name="specification">Predicate based on specification</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A entity</returns>
	Task<TEntity?> LastOrDefaultAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default );

	/// <summary>
	/// Get the last element of collection that is satisfyed by predicate
	/// </summary>
	/// <param name="predicate">Predicate</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A entity</returns>
	Task<TEntity?> LastOrDefaultAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );

	/// <summary>
	/// Get the first element of collection that is satisfyed by predicate
	/// </summary>
	/// <param name="specification">Predicate based on specification</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A entity</returns>
	/// <exception cref="InvalidOperationException">Thrown when no element is found</exception>
	Task<TEntity> FirstAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default );

	/// <summary>
	/// Get the first element of collection that is satisfyed by predicate
	/// </summary>
	/// <param name="predicate">Predicate based on expression</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A entity</returns>
	/// <exception cref="InvalidOperationException">Thrown when no element is found</exception>
	Task<TEntity> FirstAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );

	/// <summary>
	/// Get the last element of collection that is satisfyed by predicate
	/// </summary>
	/// <param name="specification">Predicate based on specification</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A entity</returns>
	/// <exception cref="InvalidOperationException">Thrown when no element is found</exception>
	Task<TEntity> LastAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default );

	/// <summary>
	/// Get the last element of collection that is satisfyed by predicate
	/// </summary>
	/// <param name="predicate">Predicate based on expression</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A entity</returns>
	/// <exception cref="InvalidOperationException">Thrown when no element is found</exception>
	Task<TEntity> LastAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );

	/// <summary>
	/// Searchs for all elements that is satisfyed by predicate and paginate
	/// </summary>
	/// <param name="filterPredicate">Predicate based on expression</param>
	/// <param name="pagination">Pagination object with page number and page size</param>
	/// <param name="orderPredicate">Predicate for ordering</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A IPaginated object with collection</returns>
	Task<IPaginated<TEntity>> SearchPaginatedAsync( Expression<Func<TEntity, bool>> filterPredicate, Pagination pagination, Expression<Func<TEntity, bool>>? orderPredicate = null, CancellationToken cancellationToken = default );

	/// <summary>
	/// Searchs if all element is satisfyed by predicate
	/// </summary>
	/// <param name="specification">Predicate based on specification</param>
	/// <param name="cancellationToken"></param>
	/// <returns>A value that represents the answer</returns>
	Task<bool> AllAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default );

	/// <summary>
	/// Searchs if all element is satisfyed by predicate
	/// </summary>
	/// <param name="predicate">Predicate</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A value that represents the answer</returns>
	Task<bool> AllAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );

	/// <summary>
	/// Get the entire collection
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The entire collection</returns>
	Task<IEnumerable<TEntity>> ToListAsync( CancellationToken cancellationToken = default );
}
