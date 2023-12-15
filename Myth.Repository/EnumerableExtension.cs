using Myth.Interfaces.Repositories.Results;
using Myth.Repositories.Results;
using Myth.ValueObjects;

namespace Myth.Extensions {

    public static class EnumerableExtension {

        public static IPaginated<TEntity> AsPaginated<TEntity>( this IEnumerable<TEntity> itens, int take = 0, int skip = 0 ) {
            var totalItens = itens.Count( );
            var pageSize = take > 0 ? take : totalItens;
            var pageNumber = ( skip > 0 ? ( skip / pageSize ) : 0 ) + 1;
            var totalPages = totalItens > 0 ? ( int )Math.Ceiling( decimal.Divide( totalItens, ( pageSize > 0 ? pageSize : totalItens ) ) ) : 0;
            var itensProcessed = itens.Skip( skip ).Take( pageSize ).ToList( );
            var paginatedResult = new Paginated<TEntity>( pageNumber, pageSize, totalItens, totalPages, itensProcessed );
            return paginatedResult;
        }

        public static IPaginated<TEntity> AsPaginated<TEntity>( this IEnumerable<TEntity> itens, Pagination pagination ) {
            return itens.AsPaginated( pagination.PageNumber, pagination.GetPagesToSkip( ) );
        }
    }
}