using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Myth.Contexts;
using Myth.Exceptions;
using Myth.Interfaces.Repositories.EntityFramework;

namespace Myth.Repositories.EntityFramework.Base;

public abstract class BaseUnitOfWorkRepository( BaseContext context ) : IUnitOfWorkRepository, IDisposable, IAsyncDisposable {
	protected readonly BaseContext _context = context;
	private IDbContextTransaction? _transaction;
	private bool _transactionAttempted;

	/// <summary>
	/// Starts a new transaction asynchronously.
	/// When the database provider does not support transactions (e.g., InMemory), the exception is
	/// silently ignored so that handlers written for real databases remain testable without modification.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A task that represents the asynchronous operation</returns>
	public async Task BeginTransactionAsync( CancellationToken cancellationToken = default ) {
		_transactionAttempted = true;
		try {
			_transaction = await _context.Database.BeginTransactionAsync( cancellationToken );
		} catch ( InvalidOperationException ) {
			// Provider does not support transactions (e.g., InMemory). No-op so that
			// Commit/Rollback/Savepoint calls below are also silently skipped.
		}
	}

	/// <summary>
	/// Commits the current transaction, applying all changes made during the transaction.
	/// No-ops when the provider does not support transactions.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A task that represents the asynchronous operation</returns>
	/// <exception cref="NoAvailableTransactionException">Thrown when <see cref="BeginTransactionAsync"/> was never called</exception>
	public async Task CommitAsync( CancellationToken cancellationToken = default ) {
		if ( _transaction is null ) {
			if ( !_transactionAttempted )
				throw new NoAvailableTransactionException( );
			return;
		}

		try {
			await _transaction.CommitAsync( cancellationToken );
		} catch ( InvalidOperationException ) when ( _transactionAttempted ) {
			// Provider does not support commit (e.g., InMemory no-op transaction)
		}
	}

	/// <summary>
	/// Rolls back the current transaction, undoing all changes made during the transaction.
	/// No-ops when the provider does not support transactions.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A task that represents the asynchronous operation</returns>
	/// <exception cref="NoAvailableTransactionException">Thrown when <see cref="BeginTransactionAsync"/> was never called</exception>
	public async Task RollbackAsync( CancellationToken cancellationToken = default ) {
		if ( _transaction is null ) {
			if ( !_transactionAttempted )
				throw new NoAvailableTransactionException( );
			return;
		}

		try {
			await _transaction.RollbackAsync( cancellationToken );
		} catch ( InvalidOperationException ) when ( _transactionAttempted ) {
			// Provider does not support rollback (e.g., InMemory no-op transaction)
		}
	}

	/// <summary>
	/// Creates a savepoint within the current transaction with the specified name.
	/// No-ops when the provider does not support savepoints (e.g., InMemory).
	/// </summary>
	/// <param name="savepoint">The name of the savepoint to create</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A task that represents the asynchronous operation</returns>
	/// <exception cref="NoAvailableTransactionException">Thrown when <see cref="BeginTransactionAsync"/> was never called</exception>
	public async Task CreateSavepointAsync( string savepoint, CancellationToken cancellationToken = default ) {
		if ( _transaction is null ) {
			if ( !_transactionAttempted )
				throw new NoAvailableTransactionException( );
			return;
		}

		try {
			await _transaction.CreateSavepointAsync( savepoint, cancellationToken );
		} catch ( InvalidOperationException ) when ( _transactionAttempted ) {
			// Provider does not support savepoints (e.g., InMemory)
		} catch ( NotSupportedException ) when ( _transactionAttempted ) {
			// Provider explicitly reports savepoints as not supported
		}
	}

	/// <summary>
	/// Rolls back the transaction to the specified savepoint, undoing changes made after that point.
	/// No-ops when the provider does not support savepoints (e.g., InMemory).
	/// </summary>
	/// <param name="savepoint">The name of the savepoint to roll back to</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A task that represents the asynchronous operation</returns>
	/// <exception cref="NoAvailableTransactionException">Thrown when <see cref="BeginTransactionAsync"/> was never called</exception>
	public async Task RollbackToSavepointAsync( string savepoint, CancellationToken cancellationToken = default ) {
		if ( _transaction is null ) {
			if ( !_transactionAttempted )
				throw new NoAvailableTransactionException( );
			return;
		}

		try {
			await _transaction.RollbackToSavepointAsync( savepoint, cancellationToken );
		} catch ( InvalidOperationException ) when ( _transactionAttempted ) {
			// Provider does not support savepoints (e.g., InMemory)
		} catch ( NotSupportedException ) when ( _transactionAttempted ) {
			// Provider explicitly reports savepoints as not supported
		}
	}

	/// <summary>
	/// Saves all changes made in the context to the database
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A task that represents the asynchronous operation containing the number of affected rows</returns>
	public virtual Task<int> SaveChangesAsync( CancellationToken cancellationToken = default ) =>
		_context.SaveChangesAsync( cancellationToken );

	/// <summary>
	/// Executes a raw SQL query against the database
	/// </summary>
	/// <param name="query">The raw SQL query to execute</param>
	/// <param name="parameters">The parameters to be used in the query</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A task that represents the asynchronous operation containing the number of affected rows</returns>
	[ExcludeFromCodeCoverage]
	public virtual Task<int> ExecuteSqlAsync( string query, IEnumerable<object>? parameters = null, CancellationToken cancellationToken = default ) {
		parameters ??= [ ];
		return _context.Database.ExecuteSqlRawAsync( query, parameters, cancellationToken );
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources
	/// </summary>
	public void Dispose( ) {
		_transaction?.Dispose( );
		_context?.Dispose( );
		GC.SuppressFinalize( this );
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously
	/// </summary>
	/// <returns>A task that represents the asynchronous dispose operation</returns>
	public ValueTask DisposeAsync( ) => DisposeAsyncCore( );

	/// <summary>
	/// Performs the core asynchronous dispose logic for the repository
	/// </summary>
	/// <returns>A task that represents the asynchronous dispose operation</returns>
	protected virtual async ValueTask DisposeAsyncCore( ) {
		if ( _transaction is not null )
			await _transaction.DisposeAsync( );

		if ( _context is not null )
			await _context.DisposeAsync( );

		GC.SuppressFinalize( this );
	}
}
