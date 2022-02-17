namespace Myth.Interfaces.Repositories.Base {

    public interface IReadWriteRepositoryAsync<TEntity> : IReadRepositoryAsync<TEntity>, IWriteRepositoryAsync<TEntity> {
    }
}
