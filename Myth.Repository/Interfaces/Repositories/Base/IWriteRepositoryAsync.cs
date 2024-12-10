namespace Myth.Interfaces.Repositories.Base;

public interface IWriteRepositoryAsync<TEntity> : IRepository, IAsyncDisposable {

	/// <summary>
	/// Insert a new element in collection
	/// </summary>
	/// <param name="entity">Element to insert</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A task</returns>
	Task AddAsync( TEntity entity, CancellationToken cancellationToken = default );

	/// <summary>
	/// Insert a list of new elements in collection
	/// </summary>
	/// <param name="entity">Elements to insert</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A task</returns>
	Task AddRangeAsync( IEnumerable<TEntity> entity, CancellationToken cancellationToken = default );

	/// <summary>
	/// Update a existing element in collection
	/// </summary>
	/// <param name="entity">Element to update</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A task</returns>
	Task UpdateAsync( TEntity entity, CancellationToken cancellationToken = default );

	/// <summary>
	/// Update a list of existing elements in collection
	/// </summary>
	/// <param name="entities">Elements to update</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A task</returns>
	Task UpdateRangeAsync( IEnumerable<TEntity> entities, CancellationToken cancellationToken = default );

	/// <summary>
	/// Remove a existing element in collection
	/// </summary>
	/// <param name="entity">Element to remove</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A task</returns>
	Task RemoveAsync( TEntity entity, CancellationToken cancellationToken = default );

	/// <summary>
	/// Remove a list of elements in collection
	/// </summary>
	/// <param name="entities">Elements to remove</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A task</returns>
	Task RemoveRangeAsync( IEnumerable<TEntity> entities, CancellationToken cancellationToken = default );
}