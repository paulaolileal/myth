using Myth.Interfaces.Repositories.Results;
using System.Linq.Expressions;

namespace Myth.Interfaces.Repositories.Base {

    public interface IReadRepositoryAsync<TEntity> : IRepository {

        IQueryable<TEntity> Where( ISpec<TEntity> specification );

        IQueryable<TEntity> Where( Expression<Func<TEntity, bool>> predicate );

        IQueryable<TEntity> AsQueryable( );

        Task<List<TEntity>> SearchAsync( ISpec<TEntity> specification, CancellationToken cancellationToken );

        Task<List<TEntity>> SearchAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );

        Task<IPaginated<TEntity>> SearchPaginatedAsync( ISpec<TEntity> specification, CancellationToken cancellationToken );

        Task<IPaginated<TEntity>> SearchPaginatedAsync( Expression<Func<TEntity, bool>> predicate, int take = 0, int skip = 0, CancellationToken cancellationToken = default );

        Task<int> CountAsync( ISpec<TEntity> specification, CancellationToken cancellationToken );

        Task<int> CountAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );

        Task<bool> AnyAsync( ISpec<TEntity> specification, CancellationToken cancellationToken );

        Task<bool> AnyAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );

        Task<TEntity?> FirstOrDefaultAsync( ISpec<TEntity> specification, CancellationToken cancellationToken );

        Task<TEntity?> FirstOrDefaultAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );

        Task<TEntity?> LastOrDefaultAsync( ISpec<TEntity> specification, CancellationToken cancellationToken );

        Task<TEntity?> LastOrDefaultAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );

        Task<bool> AllAsync( ISpec<TEntity> specification, CancellationToken cancellationToken );

        Task<bool> AllAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );

        Task<List<TEntity>> ToListAsync( CancellationToken cancellationToken );
    }
}