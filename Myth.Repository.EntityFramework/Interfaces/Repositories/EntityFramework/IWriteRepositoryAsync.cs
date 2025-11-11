namespace Myth.Interfaces.Repositories.EntityFramework;

public interface IWriteRepositoryAsync<TEntity> : Base.IWriteRepositoryAsync<TEntity> {

	/// <summary>
	/// Tracks a entity back to context
	/// </summary>
	/// <param name="entity">The entity</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A task</returns>
	Task AttachAsync( TEntity entity, CancellationToken cancellationToken = default );

	/// <summary>
	/// Tracks a list of entities back to context
	/// </summary>
	/// <param name="entities">The entities</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A task</returns>
	Task AttachRangeAsync( IEnumerable<TEntity> entities, CancellationToken cancellationToken = default );
}
