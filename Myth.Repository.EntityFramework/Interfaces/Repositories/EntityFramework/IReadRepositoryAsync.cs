namespace Myth.Interfaces.Repositories.EntityFramework {

    public interface IReadRepositoryAsync<TEntity> : Base.IReadRepositoryAsync<TEntity> {

        ValueTask<TEntity> FindAsync( CancellationToken cancellationToken, params object[ ] keys );

        string GetProviderName( );
    }
}