using Microsoft.EntityFrameworkCore;
using Myth.Contexts;
using Myth.Interfaces.Repositories.EntityFramework;

namespace Myth.Repositories.EntityFramework;

public partial class ReadRepositoryAsync<TEntity>( BaseContext context )
	: IReadRepositoryAsync<TEntity> where TEntity : class {
	protected readonly BaseContext _context = context;

	public virtual IQueryable<TEntity> AsQueryable( ) =>
		_context
			.Set<TEntity>( )
			.AsQueryable( );

	public virtual IEnumerable<TEntity> AsEnumerable( ) =>
		_context
			.Set<TEntity>( )
			.AsEnumerable( );

	public virtual async Task<IEnumerable<TEntity>> ToListAsync( CancellationToken cancellationToken = default ) {
		var result = await _context
			.Set<TEntity>( )
			.ToListAsync( cancellationToken );

		return result.AsEnumerable( );
	}

	public virtual string? GetProviderName( ) => _context.Database.ProviderName;

	public ValueTask DisposeAsync( ) => DisposeAsyncCore( );

	protected virtual async ValueTask DisposeAsyncCore( ) {
		if ( _context is not null )
			await _context.DisposeAsync( );

		GC.SuppressFinalize( this );
	}
}