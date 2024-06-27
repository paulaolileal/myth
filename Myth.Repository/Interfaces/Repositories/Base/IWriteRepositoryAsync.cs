namespace Myth.Interfaces.Repositories.Base;

public interface IWriteRepositoryAsync<TEntity> : IRepository, IAsyncDisposable {

	Task AddAsync( TEntity entity, CancellationToken cancellationToken = default );

	Task AddRangeAsync( IEnumerable<TEntity> entity, CancellationToken cancellationToken = default );

	Task UpdateAsync( TEntity entity, CancellationToken cancellationToken = default );

	Task UpdateRangeAsync( IEnumerable<TEntity> entities, CancellationToken cancellationToken = default );

	Task RemoveAsync( TEntity entity, CancellationToken cancellationToken = default );

	Task RemoveRangeAsync( IEnumerable<TEntity> entities, CancellationToken cancellationToken = default );
}