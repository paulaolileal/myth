using Microsoft.EntityFrameworkCore;
using Myth.Extensions;
using Myth.Interfaces.Repositories.EntityFramework;
using Myth.Interfaces.Repositories.Results;
using System.Linq.Expressions;

namespace Myth.Repositories.EntityFramework;

public partial class ReadRepositoryAsync<TEntity> : IReadRepositoryAsync<TEntity> where TEntity : class {

	public virtual Task<List<TEntity>> SearchAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default ) =>
		_context
			.Set<TEntity>( )
			.Where( predicate )
			.AsQueryable( )
			.ToListAsync( cancellationToken );

	public virtual async Task<IPaginated<TEntity>> SearchPaginatedAsync( Expression<Func<TEntity, bool>> predicate, int take = 0, int skip = 0, CancellationToken cancellationToken = default ) {
		var itens = await _context
			.Set<TEntity>( )
			.Where( predicate )
			.AsQueryable( )
			.ToListAsync( cancellationToken );

		var totalItens = await _context
			.Set<TEntity>( )
			.AsQueryable( )
			.CountAsync( cancellationToken );

		return itens.AsPaginated( totalItens, take, skip );
	}

	public virtual IQueryable<TEntity> Where( Expression<Func<TEntity, bool>> predicate ) =>
		_context
			.Set<TEntity>( )
			.Where( predicate )
			.AsQueryable( );

	public virtual Task<int> CountAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default ) =>
		_context
			.Set<TEntity>( )
			.CountAsync( predicate, cancellationToken );

	public virtual Task<bool> AnyAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default ) =>
		_context
			.Set<TEntity>( )
			.AnyAsync( predicate, cancellationToken );

	public virtual Task<bool> AllAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default ) =>
		_context
			.Set<TEntity>( )
			.AllAsync( predicate, cancellationToken );

	public virtual Task<TEntity?> FirstOrDefaultAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default ) =>
		_context
			.Set<TEntity>( )
			.FirstOrDefaultAsync( predicate, cancellationToken );

	public virtual Task<TEntity?> LastOrDefaultAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default ) =>
		_context
			.Set<TEntity>( )
			.LastOrDefaultAsync( predicate, cancellationToken );
}