using Microsoft.EntityFrameworkCore;
using Myth.Contexts;
using Myth.Interfaces.Repositories.EntityFramework;

namespace Myth.Repositories.EntityFramework {

    public partial class ReadRepositoryAsync<TEntity> : IReadRepositoryAsync<TEntity> where TEntity : class {
        
        protected readonly BaseContext _context;

        public ReadRepositoryAsync( BaseContext context ) => _context = context;

        public virtual ValueTask<TEntity> FindAsync( CancellationToken cancellationToken, params object[ ] keys ) =>
            _context.Set<TEntity>( ).FindAsync( keys, cancellationToken );

        public virtual IQueryable<TEntity> AsQueryable( ) =>
            _context.Set<TEntity>( ).AsQueryable( );

        public virtual Task<List<TEntity>> ToListAsync( CancellationToken cancellationToken = default ) =>
            _context.Set<TEntity>( ).ToListAsync( );

        public string GetProviderName( ) =>
            _context.Database.ProviderName;
    }
}