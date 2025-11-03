using Myth.Flow.Test.Interfaces;
using System.Threading.Tasks;

public class MockUnitOfWork : IUnitOfWork {
	public bool TransactionCommitted { get; private set; }
	public bool TransactionRolledBack { get; private set; }

	public Task BeginTransactionAsync( ) {
		TransactionCommitted = false;
		TransactionRolledBack = false;
		return Task.CompletedTask;
	}

	public Task CommitAsync( ) {
		TransactionCommitted = true;
		return Task.CompletedTask;
	}

	public Task RollbackAsync( ) {
		TransactionRolledBack = true;
		return Task.CompletedTask;
	}
}