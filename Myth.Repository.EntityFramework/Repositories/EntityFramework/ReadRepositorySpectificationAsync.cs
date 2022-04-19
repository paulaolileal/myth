using Microsoft.EntityFrameworkCore;
using Myth.Interfaces;
using Myth.Interfaces.Repositories.EntityFramework;
using Myth.Interfaces.Repositories.Results;
using Myth.Repositories.Results;

namespace Myth.Repositories.EntityFramework {

    public partial class ReadRepositoryAsync<TEntity> : IReadRepositoryAsync<TEntity> where TEntity : class {

        public virtual Task<List<TEntity>> SearchAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default ) =>
            specification.Prepare( _context.Set<TEntity>( ).AsQueryable( ) ).ToListAsync( cancellationToken );

        public virtual async Task<IPaginated<TEntity>> SearchPaginatedAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default ) {
            var itens = await specification.Prepare( _context.Set<TEntity>( ).AsQueryable( ) ).ToListAsync( cancellationToken );
            var totalItens = await specification.Filtered( _context.Set<TEntity>( ).AsQueryable( ) ).CountAsync( cancellationToken );
            var pageSize = specification.ItensTaked > 0 ? specification.ItensTaked : totalItens;
            var pageNumber = ( specification.ItensSkiped > 0 ? ( specification.ItensSkiped / pageSize ) : 0 ) + 1;
            var totalPages = ( int )Math.Ceiling( decimal.Divide( totalItens, ( pageSize > 0 ? pageSize : totalItens ) ) );
            var paginatedResult = new Paginated<TEntity>( pageNumber, pageSize, totalItens, totalPages, itens );
            return paginatedResult;
        }

        public virtual IQueryable<TEntity> Where( ISpec<TEntity> specification ) =>
            _context.Set<TEntity>( ).Where( specification.Predicate );

        public virtual Task<int> CountAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default ) =>
            _context.Set<TEntity>( ).CountAsync( specification.Predicate, cancellationToken );

        public virtual Task<bool> AnyAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default ) =>
            _context.Set<TEntity>( ).AnyAsync( specification.Predicate, cancellationToken );

        public virtual Task<TEntity> FirstOrDefaultAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default ) =>
            _context.Set<TEntity>( ).FirstOrDefaultAsync( specification.Predicate, cancellationToken );

        public virtual Task<TEntity> LastOrDefaultAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default ) =>
            _context.Set<TEntity>( ).LastOrDefaultAsync( specification.Predicate, cancellationToken );

        public virtual Task<bool> AllAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default ) =>
            _context.Set<TEntity>( ).AllAsync( specification.Predicate, cancellationToken );
    }
}