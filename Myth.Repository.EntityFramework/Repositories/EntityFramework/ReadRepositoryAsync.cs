using Microsoft.EntityFrameworkCore;
using Myth.Contexts;
using Myth.Interfaces.Repositories.EntityFramework;

namespace Myth.Repositories.EntityFramework {

	public partial class ReadRepositoryAsync<TEntity> : IReadRepositoryAsync<TEntity> where TEntity : class {
		protected readonly BaseContext _context;

		public ReadRepositoryAsync( BaseContext context ) => _context = context;

		public virtual IQueryable<TEntity> AsQueryable( ) =>
			_context
				.Set<TEntity>( )
				.AsQueryable( );

		public virtual IEnumerable<TEntity> AsEnumerable( ) =>
			_context
				.Set<TEntity>( )
				.AsEnumerable( );

		public virtual Task<List<TEntity>> ToListAsync( CancellationToken cancellationToken = default ) =>
			_context
			.Set<TEntity>( )
			.ToListAsync( cancellationToken );

		public string? GetProviderName( ) => _context.Database.ProviderName;

		public ValueTask DisposeAsync( ) => DisposeAsyncCore( );

		protected virtual async ValueTask DisposeAsyncCore( ) {
			if ( _context is not null )
				await _context.DisposeAsync( );

			GC.SuppressFinalize( this );
		}
	}
}