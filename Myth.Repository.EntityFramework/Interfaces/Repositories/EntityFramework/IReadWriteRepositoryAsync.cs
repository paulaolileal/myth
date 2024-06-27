namespace Myth.Interfaces.Repositories.EntityFramework;

public interface IReadWriteRepositoryAsync<TEntity> : IReadRepositoryAsync<TEntity>, IWriteRepositoryAsync<TEntity> {
}