namespace Myth.Interfaces.Repositories.Base {

    public interface IWriteRepositoryAsync<TEntity> : IRepository {

        Task AddAsync( TEntity entity, CancellationToken cancellationToken );

        Task AddRangeAsync( IEnumerable<TEntity> entity, CancellationToken cancellationToken );

        Task UpdateAsync( TEntity entity, CancellationToken cancellationToken );

        Task UpdateRangeAsync( IEnumerable<TEntity> entities, CancellationToken cancellationToken );

        Task RemoveAsync( TEntity entity, CancellationToken cancellationToken );

        Task RemoveRangeAsync( IEnumerable<TEntity> entities, CancellationToken cancellationToken );
    }
}