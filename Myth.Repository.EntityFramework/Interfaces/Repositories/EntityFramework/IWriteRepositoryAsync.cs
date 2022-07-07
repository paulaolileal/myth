namespace Myth.Interfaces.Repositories.EntityFramework {

    public interface IWriteRepositoryAsync<TEntity> : Base.IWriteRepositoryAsync<TEntity> {

        Task AttachAsync( TEntity entity, CancellationToken cancellationToken );

        Task AttachRangeAsync( IEnumerable<TEntity> entities, CancellationToken cancellationToken );

        Task<int> ExecuteSqlAsync( string query, IEnumerable<object> parameters, CancellationToken cancellationToken );

        Task<int> SaveChangesAsync( CancellationToken cancellationToken );
    }
}