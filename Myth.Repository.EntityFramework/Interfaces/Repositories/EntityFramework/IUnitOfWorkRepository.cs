namespace Myth.Interfaces.Repositories.EntityFramework {

	public interface IUnitOfWorkRepository : IAsyncDisposable {

		Task BeginTransactionAsync( CancellationToken cancellationToken = default );

		Task CommitAsync( CancellationToken cancellationToken = default );

		Task CreateSavepointAsync( string savepoint, CancellationToken cancellationToken = default );

		Task<int> ExecuteSqlAsync( string query, IEnumerable<object>? parameters = null, CancellationToken cancellationToken = default );

		Task RollbackAsync( CancellationToken cancellationToken = default );

		Task RollbackToSavepointAsync( string savepoint, CancellationToken cancellationToken = default );

		Task<int> SaveChangesAsync( CancellationToken cancellationToken = default );
	}
}