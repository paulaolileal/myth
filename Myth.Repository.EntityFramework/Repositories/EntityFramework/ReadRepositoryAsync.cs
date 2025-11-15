using Microsoft.EntityFrameworkCore;
using Myth.Contexts;
using Myth.Interfaces.Repositories.EntityFramework;

namespace Myth.Repositories.EntityFramework;

public partial class ReadRepositoryAsync<TEntity>( BaseContext context )
	: IReadRepositoryAsync<TEntity> where TEntity : class {
	protected readonly BaseContext _context = context;

	/// <summary>
	/// Return the original collection as queryable
	/// </summary>
	/// <returns>The queryable collection</returns>
	public virtual IQueryable<TEntity> AsQueryable( ) =>
		_context
			.Set<TEntity>( )
			.AsQueryable( );

	/// <summary>
	/// Return the original collection as enumerable
	/// </summary>
	/// <returns>The enumerable collection</returns>
	public virtual IEnumerable<TEntity> AsEnumerable( ) =>
		_context
			.Set<TEntity>( )
			.AsEnumerable( );

	/// <summary>
	/// Get the entire collection
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The entire collection</returns>
	public virtual async Task<IEnumerable<TEntity>> ToListAsync( CancellationToken cancellationToken = default ) {
		var result = await _context
			.Set<TEntity>( )
			.ToListAsync( cancellationToken );

		return result.AsEnumerable( );
	}

	/// <summary>
	/// Get the name of provider being used
	/// </summary>
	/// <returns>A value with the name</returns>
	public virtual string? GetProviderName( ) => _context.Database.ProviderName;

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously
	/// </summary>
	/// <returns>A task that represents the asynchronous dispose operation</returns>
	public ValueTask DisposeAsync( ) => DisposeAsyncCore( );

	/// <summary>
	/// Performs the core asynchronous dispose operation
	/// </summary>
	/// <returns>A task that represents the asynchronous dispose operation</returns>
	protected virtual async ValueTask DisposeAsyncCore( ) {
		if ( _context is not null )
			await _context.DisposeAsync( );

		GC.SuppressFinalize( this );
	}
}
