namespace Myth.Interfaces.Repositories.EntityFramework {

	public interface IWriteRepositoryAsync<TEntity> : Base.IWriteRepositoryAsync<TEntity> {

		Task AttachAsync( TEntity entity, CancellationToken cancellationToken = default );

		Task AttachRangeAsync( IEnumerable<TEntity> entities, CancellationToken cancellationToken = default );

		Task<int> ExecuteSqlAsync( string query, IEnumerable<object>? parameters = null, CancellationToken cancellationToken = default );

		Task<int> SaveChangesAsync( CancellationToken cancellationToken = default );
	}
}