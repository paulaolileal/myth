namespace Myth.Interfaces.Repositories.EntityFramework {

	public interface IWriteRepositoryAsync<TEntity> : Base.IWriteRepositoryAsync<TEntity> {

		Task AttachAsync( TEntity entity, CancellationToken cancellationToken = default );

		Task AttachRangeAsync( IEnumerable<TEntity> entities, CancellationToken cancellationToken = default );
	}
}