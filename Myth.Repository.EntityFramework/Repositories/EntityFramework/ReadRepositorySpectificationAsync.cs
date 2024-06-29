using Microsoft.EntityFrameworkCore;
using Myth.Extensions;
using Myth.Interfaces;
using Myth.Interfaces.Repositories.EntityFramework;
using Myth.Interfaces.Repositories.Results;

namespace Myth.Repositories.EntityFramework;

public partial class ReadRepositoryAsync<TEntity> : IReadRepositoryAsync<TEntity> where TEntity : class {

	public virtual async Task<IEnumerable<TEntity>> SearchAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default ) {
		var result = await _context
			.Set<TEntity>( )
			.AsQueryable( )
			.Specify( specification )
			.ToListAsync( cancellationToken );

		return result.AsEnumerable();
	}

	public virtual async Task<IPaginated<TEntity>> SearchPaginatedAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default ) =>
		await Task.Run( ( ) => {
			var processedEntitySet = _context
				.Set<TEntity>( )
				.Where( specification );

			var totalItems = processedEntitySet.Count( );

			processedEntitySet = specification.Sorted( processedEntitySet.AsQueryable( ) );

			processedEntitySet = specification.Processed( processedEntitySet.AsQueryable( ) );

			var items = processedEntitySet.ToList( );

			return items.AsPaginated(
				totalItems,
				specification.ItemsTaked,
				specification.ItemsSkiped );
		}, cancellationToken );

	public virtual IQueryable<TEntity> Where( ISpec<TEntity> specification ) =>
		_context
			.Set<TEntity>( )
			.Where( specification.Predicate );

	public virtual Task<int> CountAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default ) =>
		_context
			.Set<TEntity>( )
			.CountAsync( specification.Predicate, cancellationToken );

	public virtual Task<bool> AnyAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default ) =>
		_context
			.Set<TEntity>( )
			.AnyAsync( specification.Predicate, cancellationToken );

	public virtual Task<TEntity?> FirstOrDefaultAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default ) =>
		_context
			.Set<TEntity>( )
			.FirstOrDefaultAsync( specification.Predicate, cancellationToken );

	public virtual Task<TEntity?> LastOrDefaultAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default ) =>
		_context
			.Set<TEntity>( )
			.LastOrDefaultAsync( specification.Predicate, cancellationToken );

	public virtual Task<bool> AllAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default ) =>
		_context
			.Set<TEntity>( )
			.AllAsync( specification.Predicate, cancellationToken );
}