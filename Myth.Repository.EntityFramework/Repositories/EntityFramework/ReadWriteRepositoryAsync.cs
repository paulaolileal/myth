using Myth.Contexts;
using Myth.Interfaces;
using Myth.Interfaces.Repositories.EntityFramework;
using Myth.Interfaces.Repositories.Results;
using System.Linq.Expressions;

namespace Myth.Repositories.EntityFramework {

    public abstract class ReadWriteRepositoryAsync<TEntity> : IReadWriteRepositoryAsync<TEntity> where TEntity : class {
        private readonly IReadRepositoryAsync<TEntity> _readRepository;

        private readonly IWriteRepositoryAsync<TEntity> _writeRepository;

        public ReadWriteRepositoryAsync( BaseContext context ) {
            _readRepository = new ReadRepositoryAsync<TEntity>( context );
            _writeRepository = new WriteRepositoryAsync<TEntity>( context );
        }

        public virtual Task AddAsync( TEntity entity, CancellationToken cancellationToken = default ) =>
            _writeRepository.AddAsync( entity, cancellationToken );

        public virtual Task AddRangeAsync( IEnumerable<TEntity> entities, CancellationToken cancellationToken = default ) =>
            _writeRepository.AddRangeAsync( entities, cancellationToken );

        public virtual Task<bool> AllAsync( ISpec<TEntity> spec, CancellationToken cancellationToken = default ) =>
            _readRepository.AllAsync( spec, cancellationToken );

        public Task<bool> AllAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default ) =>
            _readRepository.AllAsync( predicate, cancellationToken );

        public virtual Task<bool> AnyAsync( ISpec<TEntity> spec, CancellationToken cancellationToken = default ) =>
            _readRepository.AnyAsync( spec, cancellationToken );

        public Task<bool> AnyAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default ) =>
            _readRepository.AnyAsync( predicate, cancellationToken );

        public virtual IQueryable<TEntity> AsQueryable( ) =>
            _readRepository.AsQueryable( );

        public virtual Task AttachAsync( TEntity entity, CancellationToken cancellationToken = default ) =>
            _writeRepository.AttachAsync( entity, cancellationToken );

        public virtual Task AttachRangeAsync( IEnumerable<TEntity> entities, CancellationToken cancellationToken = default ) =>
            _writeRepository.AttachRangeAsync( entities, cancellationToken );

        public virtual Task<int> CountAsync( ISpec<TEntity> spec, CancellationToken cancellationToken = default ) =>
            _readRepository.CountAsync( spec, cancellationToken );

        public Task<int> CountAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default ) =>
            _readRepository.CountAsync( predicate, cancellationToken );

        public virtual ValueTask<TEntity> FindAsync( CancellationToken cancellationToken = default, params object[ ] keys ) =>
            _readRepository.FindAsync( cancellationToken, keys );

        public virtual Task<TEntity> FirstOrDefaultAsync( ISpec<TEntity> spec, CancellationToken cancellationToken = default ) =>
            _readRepository.FirstOrDefaultAsync( spec, cancellationToken );

        public Task<TEntity> FirstOrDefaultAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default ) =>
            _readRepository.FirstOrDefaultAsync( predicate, cancellationToken );

        public virtual string GetProviderName( ) =>
            _readRepository.GetProviderName( );

        public virtual Task<TEntity> LastOrDefaultAsync( ISpec<TEntity> spec, CancellationToken cancellationToken = default ) =>
            _readRepository.LastOrDefaultAsync( spec, cancellationToken );

        public Task<TEntity> LastOrDefaultAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default ) =>
            _readRepository.LastOrDefaultAsync( predicate, cancellationToken );

        public virtual Task RemoveAsync( TEntity entity, CancellationToken cancellationToken = default ) =>
            _writeRepository.RemoveAsync( entity, cancellationToken );

        public virtual Task RemoveRangeAsync( IEnumerable<TEntity> entities, CancellationToken cancellationToken = default ) =>
            _writeRepository.RemoveRangeAsync( entities, cancellationToken );

        public virtual Task<int> SaveChangesAsync( CancellationToken cancellationToken = default ) =>
            _writeRepository.SaveChangesAsync( cancellationToken );

        public virtual Task<List<TEntity>> SearchAsync( ISpec<TEntity> spec, CancellationToken cancellationToken = default ) =>
            _readRepository.SearchAsync( spec, cancellationToken );

        public Task<List<TEntity>> SearchAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default ) =>
            _readRepository.SearchAsync( predicate, cancellationToken );

        public virtual Task<IPaginated<TEntity>> SearchPaginatedAsync( ISpec<TEntity> spec, CancellationToken cancellationToken = default ) =>
            _readRepository.SearchPaginatedAsync( spec, cancellationToken );

        public Task<IPaginated<TEntity>> SearchPaginatedAsync( Expression<Func<TEntity, bool>> predicate, int take = 0, int skip = 0, CancellationToken cancellationToken = default ) =>
            _readRepository.SearchPaginatedAsync( predicate, take, skip, cancellationToken );

        public virtual Task<List<TEntity>> ToListAsync( CancellationToken cancellationToken = default ) =>
            _readRepository.ToListAsync( cancellationToken );

        public virtual Task UpdateAsync( TEntity entity, CancellationToken cancellationToken = default ) =>
            _writeRepository.UpdateAsync( entity, cancellationToken );

        public virtual Task UpdateRangeAsync( IEnumerable<TEntity> entities, CancellationToken cancellationToken = default ) =>
            _writeRepository.UpdateRangeAsync( entities, cancellationToken );

        public virtual IQueryable<TEntity> Where( ISpec<TEntity> spec ) =>
                                            _readRepository.Where( spec );

        public IQueryable<TEntity> Where( Expression<Func<TEntity, bool>> predicate ) =>
            _readRepository.Where( predicate );
    }
}