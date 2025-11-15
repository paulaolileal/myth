namespace Myth.Interfaces.Repositories.EntityFramework;

public interface IUnitOfWorkRepository : IAsyncDisposable {

	/// <summary>
	/// Starts a new transaction
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A task</returns>
	Task BeginTransactionAsync( CancellationToken cancellationToken = default );

	/// <summary>
	/// Applys all changes in current transaction
	/// </summary>
	/// <param name="cancellationToken">Cancellation Token</param>
	/// <returns>A task</returns>
	Task CommitAsync( CancellationToken cancellationToken = default );

	/// <summary>
	/// Create a new savepoint in current transaction
	/// </summary>
	/// <param name="savepointName">Name of savepoint</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A task</returns>
	Task CreateSavepointAsync( string savepointName, CancellationToken cancellationToken = default );

	/// <summary>
	/// Executes a raw SQL query on database directly
	/// </summary>
	/// <param name="query">The SQL query</param>
	/// <param name="parameters">Parameter to be replaced in query</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The number of affected rows</returns>
	Task<int> ExecuteSqlAsync( string query, IEnumerable<object>? parameters = null, CancellationToken cancellationToken = default );

	/// <summary>
	/// Undo all changes in current transaction
	/// </summary>
	/// <param name="cancellationToken">Cacellation token </param>
	/// <returns>A task</returns>
	Task RollbackAsync( CancellationToken cancellationToken = default );

	/// <summary>
	/// Undo the changes until the savepoint
	/// </summary>
	/// <param name="savepointName">The name of savepoint</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A task</returns>
	Task RollbackToSavepointAsync( string savepointName, CancellationToken cancellationToken = default );

	/// <summary>
	/// Save all changes in context
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The number of affected rows</returns>
	Task<int> SaveChangesAsync( CancellationToken cancellationToken = default );
}
