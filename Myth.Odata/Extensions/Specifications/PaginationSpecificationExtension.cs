using Myth.Interfaces;
using Myth.ValueObjects;

namespace Myth.Extensions.Specifications {

    public static class PaginationSpecificationExtension {

        public static ISpec<T> Paginate<T>( this ISpec<T> spec, Pagination pagination ) {
            pagination.PageNumber--;

            if ( pagination.PageNumber >= 0 )
                spec = spec.Skip( pagination.PageNumber * pagination.PageSize );

            if ( pagination.PageSize > 0 )
                spec = spec.Take( pagination.PageSize );

            return spec;
        }
    }
}