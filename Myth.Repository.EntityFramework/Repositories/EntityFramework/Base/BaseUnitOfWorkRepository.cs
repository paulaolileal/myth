using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Myth.Contexts;
using Myth.Exceptions;
using Myth.Interfaces.Repositories.EntityFramework;

namespace Myth.Repositories.EntityFramework.Base;

public abstract class BaseUnitOfWorkRepository( BaseContext context ) : IUnitOfWorkRepository, IAsyncDisposable {
	protected readonly BaseContext _context = context;
	private IDbContextTransaction? _transaction;

	/// <summary>
	/// Starts a new transaction asynchronously
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A task that represents the asynchronous operation</returns>
	public async Task BeginTransactionAsync( CancellationToken cancellationToken = default ) =>
		_transaction = await _context.Database.BeginTransactionAsync( cancellationToken );

	/// <summary>
	/// Commits the current transaction, applying all changes made during the transaction
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A task that represents the asynchronous operation</returns>
	/// <exception cref="NoAvailableTransactionException">Thrown when no transaction is available</exception>
	public async Task CommitAsync( CancellationToken cancellationToken = default ) {
		if ( _transaction is null )
			throw new NoAvailableTransactionException( );

		await _transaction.CommitAsync( cancellationToken );
	}

	/// <summary>
	/// Rolls back the current transaction, undoing all changes made during the transaction
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A task that represents the asynchronous operation</returns>
	/// <exception cref="NoAvailableTransactionException">Thrown when no transaction is available</exception>
	public async Task RollbackAsync( CancellationToken cancellationToken = default ) {
		if ( _transaction is null )
			throw new NoAvailableTransactionException( );

		await _transaction.RollbackAsync( cancellationToken );
	}

	/// <summary>
	/// Creates a savepoint within the current transaction with the specified name
	/// </summary>
	/// <param name="savepoint">The name of the savepoint to create</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A task that represents the asynchronous operation</returns>
	/// <exception cref="NoAvailableTransactionException">Thrown when no transaction is available</exception>
	public async Task CreateSavepointAsync( string savepoint, CancellationToken cancellationToken = default ) {
		if ( _transaction is null )
			throw new NoAvailableTransactionException( );

		await _transaction.CreateSavepointAsync( savepoint, cancellationToken );
	}

	/// <summary>
	/// Rolls back the transaction to the specified savepoint, undoing changes made after that point
	/// </summary>
	/// <param name="savepoint">The name of the savepoint to roll back to</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A task that represents the asynchronous operation</returns>
	/// <exception cref="NoAvailableTransactionException">Thrown when no transaction is available</exception>
	public async Task RollbackToSavepointAsync( string savepoint, CancellationToken cancellationToken = default ) {
		if ( _transaction is null )
			throw new NoAvailableTransactionException( );

		await _transaction.RollbackToSavepointAsync( savepoint, cancellationToken );
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
