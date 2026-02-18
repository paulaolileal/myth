using System.Linq.Expressions;
using Myth.Contexts;
using Myth.Interfaces;
using Myth.Interfaces.Repositories.EntityFramework;
using Myth.Interfaces.Results;
using Myth.ValueObjects;

namespace Myth.Repositories.EntityFramework;

public abstract class ReadWriteRepositoryAsync<TEntity>( BaseContext context ) : IDisposable, IAsyncDisposable, IReadWriteRepositoryAsync<TEntity> where TEntity : class {
	protected readonly BaseContext _context = context;

	private readonly IReadRepositoryAsync<TEntity> _readRepository = new ReadRepositoryAsync<TEntity>( context );

	private readonly IWriteRepositoryAsync<TEntity> _writeRepository = new WriteRepositoryAsync<TEntity>( context );

	/// <summary>
	/// Asynchronously adds a new entity to the repository
	/// </summary>
	/// <param name="entity">The entity to add</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous add operation</returns>
	public virtual Task AddAsync( TEntity entity, CancellationToken cancellationToken = default ) =>
		_writeRepository.AddAsync( entity, cancellationToken );

	/// <summary>
	/// Asynchronously adds a collection of entities to the repository
	/// </summary>
	/// <param name="entities">The collection of entities to add</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous add operation</returns>
	public virtual Task AddRangeAsync( IEnumerable<TEntity> entities, CancellationToken cancellationToken = default ) =>
		_writeRepository.AddRangeAsync( entities, cancellationToken );

	/// <summary>
	/// Asynchronously checks if all entities satisfy the given specification
	/// </summary>
	/// <param name="spec">The specification to check against</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous operation. The task result is true if all entities satisfy the specification; otherwise, false</returns>
	public virtual Task<bool> AllAsync( ISpec<TEntity> spec, CancellationToken cancellationToken = default ) =>
		_readRepository.AllAsync( spec, cancellationToken );

	/// <summary>
	/// Asynchronously checks if all entities satisfy the given predicate
	/// </summary>
	/// <param name="predicate">The predicate to check against</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous operation. The task result is true if all entities satisfy the predicate; otherwise, false</returns>
	public virtual Task<bool> AllAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default ) =>
		_readRepository.AllAsync( predicate, cancellationToken );

	/// <summary>
	/// Asynchronously checks if any entity satisfies the given specification
	/// </summary>
	/// <param name="spec">The specification to check against</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous operation. The task result is true if any entity satisfies the specification; otherwise, false</returns>
	public virtual Task<bool> AnyAsync( ISpec<TEntity> spec, CancellationToken cancellationToken = default ) =>
		_readRepository.AnyAsync( spec, cancellationToken );

	/// <summary>
	/// Asynchronously checks if any entity satisfies the given predicate
	/// </summary>
	/// <param name="predicate">The predicate to check against</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous operation. The task result is true if any entity satisfies the predicate; otherwise, false</returns>
	public virtual Task<bool> AnyAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default ) =>
		_readRepository.AnyAsync( predicate, cancellationToken );

	/// <summary>
	/// Returns the collection as an IQueryable
	/// </summary>
	/// <returns>An IQueryable representation of the entities</returns>
	public virtual IQueryable<TEntity> AsQueryable( ) => _readRepository.AsQueryable( );

	/// <summary>
	/// Returns the collection as an IEnumerable
	/// </summary>
	/// <returns>An IEnumerable representation of the entities</returns>
	public virtual IEnumerable<TEntity> AsEnumerable( ) => _readRepository.AsEnumerable( );

	/// <summary>
	/// Asynchronously attaches an entity to the context if it's detached
	/// </summary>
	/// <param name="entity">The entity to attach</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous attach operation</returns>
	public virtual Task AttachAsync( TEntity entity, CancellationToken cancellationToken = default ) =>
		_writeRepository.AttachAsync( entity, cancellationToken );

	/// <summary>
	/// Asynchronously attaches a collection of entities to the context if they are detached
	/// </summary>
	/// <param name="entities">The collection of entities to attach</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous attach operation</returns>
	public virtual Task AttachRangeAsync( IEnumerable<TEntity> entities, CancellationToken cancellationToken = default ) =>
		_writeRepository.AttachRangeAsync( entities, cancellationToken );

	/// <summary>
	/// Asynchronously counts the number of entities that satisfy the given specification
	/// </summary>
	/// <param name="spec">The specification to count against</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous operation. The task result is the number of entities that satisfy the specification</returns>
	public virtual Task<int> CountAsync( ISpec<TEntity> spec, CancellationToken cancellationToken = default ) =>
		_readRepository.CountAsync( spec, cancellationToken );

	/// <summary>
	/// Asynchronously counts the number of entities that satisfy the given predicate
	/// </summary>
	/// <param name="predicate">The predicate to count against</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous operation. The task result is the number of entities that satisfy the predicate</returns>
	public virtual Task<int> CountAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default ) =>
		_readRepository.CountAsync( predicate, cancellationToken );

	/// <summary>
	/// Asynchronously returns the first entity that satisfies the given specification, or null if no such entity is found
	/// </summary>
	/// <param name="spec">The specification to filter by</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous operation. The task result is the first entity that satisfies the specification, or null if no entity is found</returns>
	public virtual Task<TEntity?> FirstOrDefaultAsync( ISpec<TEntity> spec, CancellationToken cancellationToken = default ) =>
		_readRepository.FirstOrDefaultAsync( spec, cancellationToken );

	/// <summary>
	/// Asynchronously returns the first entity that satisfies the given predicate, or null if no such entity is found
	/// </summary>
	/// <param name="predicate">The predicate to filter by</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous operation. The task result is the first entity that satisfies the predicate, or null if no entity is found</returns>
	public virtual Task<TEntity?> FirstOrDefaultAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default ) =>
		_readRepository.FirstOrDefaultAsync( predicate, cancellationToken );

	/// <summary>
	/// Asynchronously returns the first entity that satisfies the given specification
	/// </summary>
	/// <param name="spec">The specification to filter by</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous operation. The task result is the first entity that satisfies the specification</returns>
	/// <exception cref="InvalidOperationException">Thrown when no element is found</exception>
	public virtual Task<TEntity> FirstAsync( ISpec<TEntity> spec, CancellationToken cancellationToken = default ) =>
		_readRepository.FirstAsync( spec, cancellationToken );

	/// <summary>
	/// Asynchronously returns the first entity that satisfies the given predicate
	/// </summary>
	/// <param name="predicate">The predicate to filter by</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous operation. The task result is the first entity that satisfies the predicate</returns>
	/// <exception cref="InvalidOperationException">Thrown when no element is found</exception>
	public virtual Task<TEntity> FirstAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default ) =>
		_readRepository.FirstAsync( predicate, cancellationToken );

	/// <summary>
	/// Gets the name of the database provider being used
	/// </summary>
	/// <returns>The name of the database provider, or null if not available</returns>
	public virtual string? GetProviderName( ) => _readRepository.GetProviderName( );

	/// <summary>
	/// Asynchronously returns the last entity that satisfies the given specification, or null if no such entity is found
	/// </summary>
	/// <param name="spec">The specification to filter by</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous operation. The task result is the last entity that satisfies the specification, or null if no entity is found</returns>
	public virtual Task<TEntity?> LastOrDefaultAsync( ISpec<TEntity> spec, CancellationToken cancellationToken = default ) =>
		_readRepository.LastOrDefaultAsync( spec, cancellationToken );

	/// <summary>
	/// Asynchronously returns the last entity that satisfies the given predicate, or null if no such entity is found
	/// </summary>
	/// <param name="predicate">The predicate to filter by</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous operation. The task result is the last entity that satisfies the predicate, or null if no entity is found</returns>
	public virtual Task<TEntity?> LastOrDefaultAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default ) =>
		_readRepository.LastOrDefaultAsync( predicate, cancellationToken );

	/// <summary>
	/// Asynchronously returns the last entity that satisfies the given specification
	/// </summary>
	/// <param name="spec">The specification to filter by</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous operation. The task result is the last entity that satisfies the specification</returns>
	/// <exception cref="InvalidOperationException">Thrown when no element is found</exception>
	public virtual Task<TEntity> LastAsync( ISpec<TEntity> spec, CancellationToken cancellationToken = default ) =>
		_readRepository.LastAsync( spec, cancellationToken );

	/// <summary>
	/// Asynchronously returns the last entity that satisfies the given predicate
	/// </summary>
	/// <param name="predicate">The predicate to filter by</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous operation. The task result is the last entity that satisfies the predicate</returns>
	/// <exception cref="InvalidOperationException">Thrown when no element is found</exception>
	public virtual Task<TEntity> LastAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default ) =>
		_readRepository.LastAsync( predicate, cancellationToken );

	/// <summary>
	/// Asynchronously removes an entity from the repository
	/// </summary>
	/// <param name="entity">The entity to remove</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous remove operation</returns>
	public virtual Task RemoveAsync( TEntity entity, CancellationToken cancellationToken = default ) =>
		_writeRepository.RemoveAsync( entity, cancellationToken );

	/// <summary>
	/// Asynchronously removes a collection of entities from the repository
	/// </summary>
	/// <param name="entities">The collection of entities to remove</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous remove operation</returns>
	public virtual Task RemoveRangeAsync( IEnumerable<TEntity> entities, CancellationToken cancellationToken = default ) =>
		_writeRepository.RemoveRangeAsync( entities, cancellationToken );

	/// <summary>
	/// Asynchronously searches for entities that satisfy the given specification
	/// </summary>
	/// <param name="spec">The specification to search by</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous operation. The task result is a collection of entities that satisfy the specification</returns>
	public virtual Task<IEnumerable<TEntity>> SearchAsync( ISpec<TEntity> spec, CancellationToken cancellationToken = default ) =>
		_readRepository.SearchAsync( spec, cancellationToken );

	/// <summary>
	/// Asynchronously searches for entities that satisfy the given filter predicate and order predicate
	/// </summary>
	/// <param name="filterPredicate">The predicate to filter by</param>
	/// <param name="orderPredicate">The predicate to order by (optional)</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous operation. The task result is a collection of entities that satisfy the filter predicate</returns>
	public virtual Task<IEnumerable<TEntity>> SearchAsync( Expression<Func<TEntity, bool>> filterPredicate, Expression<Func<TEntity, bool>>? orderPredicate = null, CancellationToken cancellationToken = default ) =>
		_readRepository.SearchAsync( filterPredicate, orderPredicate, cancellationToken );

	/// <summary>
	/// Asynchronously searches for entities that satisfy the given specification with pagination
	/// </summary>
	/// <param name="spec">The specification to search by</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous operation. The task result is a paginated collection of entities that satisfy the specification</returns>
	public virtual Task<IPaginated<TEntity>> SearchPaginatedAsync( ISpec<TEntity> spec, CancellationToken cancellationToken = default ) =>
		_readRepository.SearchPaginatedAsync( spec, cancellationToken );

	/// <summary>
	/// Asynchronously searches for entities that satisfy the given predicate with pagination
	/// </summary>
	/// <param name="predicate">The predicate to filter by</param>
	/// <param name="take">The number of entities to take (0 for all)</param>
	/// <param name="skip">The number of entities to skip</param>
	/// <param name="orderPredicate">The predicate to order by (optional)</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous operation. The task result is a paginated collection of entities that satisfy the predicate</returns>
	public virtual Task<IPaginated<TEntity>> SearchPaginatedAsync( Expression<Func<TEntity, bool>> predicate, int take = 0, int skip = 0, Expression<Func<TEntity, bool>>? orderPredicate = null, CancellationToken cancellationToken = default ) =>
		_readRepository.SearchPaginatedAsync( predicate, take, skip, orderPredicate, cancellationToken );

	/// <summary>
	/// Asynchronously searches for entities that satisfy the given filter predicate with pagination using Pagination object
	/// </summary>
	/// <param name="filterPredicate">The predicate to filter by</param>
	/// <param name="pagination">The pagination object with page number and page size</param>
	/// <param name="orderPredicate">The predicate to order by (optional)</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous operation. The task result is a paginated collection of entities that satisfy the filter predicate</returns>
	public virtual Task<IPaginated<TEntity>> SearchPaginatedAsync( Expression<Func<TEntity, bool>> filterPredicate, Pagination pagination, Expression<Func<TEntity, bool>>? orderPredicate = null, CancellationToken cancellationToken = default ) =>
		_readRepository.SearchPaginatedAsync( filterPredicate, pagination, orderPredicate, cancellationToken );

	/// <summary>
	/// Asynchronously returns all entities in the repository
	/// </summary>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous operation. The task result is a collection of all entities</returns>
	public virtual Task<IEnumerable<TEntity>> ToListAsync( CancellationToken cancellationToken = default ) =>
		_readRepository.ToListAsync( cancellationToken );

	/// <summary>
	/// Asynchronously updates an existing entity in the repository
	/// </summary>
	/// <param name="entity">The entity to update</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous update operation</returns>
	public virtual Task UpdateAsync( TEntity entity, CancellationToken cancellationToken = default ) =>
		_writeRepository.UpdateAsync( entity, cancellationToken );

	/// <summary>
	/// Asynchronously updates a collection of existing entities in the repository
	/// </summary>
	/// <param name="entities">The collection of entities to update</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous update operation</returns>
	public virtual Task UpdateRangeAsync( IEnumerable<TEntity> entities, CancellationToken cancellationToken = default ) =>
		_writeRepository.UpdateRangeAsync( entities, cancellationToken );

	/// <summary>
	/// Filters entities based on the given specification
	/// </summary>
	/// <param name="spec">The specification to filter by</param>
	/// <returns>A queryable collection of entities that satisfy the specification</returns>
	public virtual IQueryable<TEntity> Where( ISpec<TEntity> spec ) => _readRepository.Where( spec );

	/// <summary>
	/// Filters entities based on the given predicate
	/// </summary>
	/// <param name="predicate">The predicate to filter by</param>
	/// <returns>A queryable collection of entities that satisfy the predicate</returns>
	public virtual IQueryable<TEntity> Where( Expression<Func<TEntity, bool>> predicate ) =>
		_readRepository.Where( predicate );

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources
	/// </summary>
	public void Dispose( ) {
		_context?.Dispose( );
		GC.SuppressFinalize( this );
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously
	/// </summary>
	/// <returns>A task that represents the asynchronous dispose operation</returns>
	public ValueTask DisposeAsync( ) => DisposeAsyncCore( );

	/// <summary>
	/// Core implementation of the asynchronous dispose pattern
	/// </summary>
	/// <returns>A task that represents the asynchronous dispose operation</returns>
	protected virtual async ValueTask DisposeAsyncCore( ) {
		if ( _writeRepository is not null )
			await _writeRepository.DisposeAsync( );

		if ( _readRepository is not null )
			await _readRepository.DisposeAsync( );

		if ( _context is not null )
			await _context.DisposeAsync( );

		GC.SuppressFinalize( this );
	}
}
