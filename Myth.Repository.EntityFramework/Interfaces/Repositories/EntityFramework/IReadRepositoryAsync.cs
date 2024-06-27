namespace Myth.Interfaces.Repositories.EntityFramework;

public interface IReadRepositoryAsync<TEntity> : Base.IReadRepositoryAsync<TEntity> {

	string? GetProviderName( );
}