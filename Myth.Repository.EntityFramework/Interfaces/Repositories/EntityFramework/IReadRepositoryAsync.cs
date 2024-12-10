namespace Myth.Interfaces.Repositories.EntityFramework;

public interface IReadRepositoryAsync<TEntity> : Base.IReadRepositoryAsync<TEntity> {

	/// <summary>
	/// Get the name of provider beeing used
	/// </summary>
	/// <returns>A value with the name</returns>
	string? GetProviderName( );
}