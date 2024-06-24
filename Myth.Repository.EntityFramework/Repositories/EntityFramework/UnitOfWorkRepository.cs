using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Myth.Contexts;
using Myth.Interfaces.Repositories.EntityFramework;
using System.Diagnostics.CodeAnalysis;

namespace Myth.Repositories.EntityFramework {

	public class UnitOfWorkRepository : IUnitOfWorkRepository, IAsyncDisposable {
		protected readonly BaseContext _context;
		private IDbContextTransaction _transaction;

		public UnitOfWorkRepository( BaseContext context ) {
			_context = context;
		}

		public async Task BeginTransactionAsync( CancellationToken cancellationToken = default ) {
			_transaction = await _context.Database.BeginTransactionAsync( cancellationToken );
		}

		public async Task CommitAsync( CancellationToken cancellationToken = default ) {
			await _transaction.CommitAsync( cancellationToken );
		}

		public async Task RollbackAsync( CancellationToken cancellationToken = default ) {
			await _transaction.RollbackAsync( cancellationToken );
		}

		public async Task CreateSavepointAsync( string savepoint, CancellationToken cancellationToken = default ) {
			await _transaction.CreateSavepointAsync( savepoint, cancellationToken );
		}

		public async Task RollbackToSavepointAsync( string savepoint, CancellationToken cancellationToken = default ) {
			await _transaction.RollbackToSavepointAsync( savepoint, cancellationToken );
		}

		public virtual Task<int> SaveChangesAsync( CancellationToken cancellationToken = default ) =>
			_context.SaveChangesAsync( cancellationToken );

		[ExcludeFromCodeCoverage]
		public virtual Task<int> ExecuteSqlAsync( string query, IEnumerable<object>? parameters = null, CancellationToken cancellationToken = default ) {
			parameters ??= [ ];
			return _context.Database.ExecuteSqlRawAsync( query, parameters, cancellationToken );
		}

		public ValueTask DisposeAsync( ) => DisposeAsyncCore( );

		protected virtual async ValueTask DisposeAsyncCore( ) {
			if ( _transaction is not null )
				await _transaction.DisposeAsync( );

			if ( _context is not null )
				await _context.DisposeAsync( );

			GC.SuppressFinalize( this );
		}
	}
}