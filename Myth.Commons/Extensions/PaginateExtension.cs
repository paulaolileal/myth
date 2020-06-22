using Myth.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Myth.Extensions {

    public static class PaginateExtension {

        public static string Paginate( this string url, int startIndex, int pageSize ) =>
             new String( url.Concat( $"?PageNumber={startIndex}&PageSize={pageSize}" ).ToArray( ) );

        public static string Paginate( this string url, Pagination pagination ) =>
             url.Paginate( pagination.PageNumber, pagination.PageSize );

        public static IEnumerable<T> Paginate<T>( this IEnumerable<T> list, int pageNumber, int pageSize ) {
            if ( pageNumber >= 0 && pageSize > 0 )
                return list.Skip( pageNumber ).Take( pageSize );
            return list;
        }

        public static IEnumerable<T> Paginate<T>( this IEnumerable<T> list, Pagination pagination ) {
            if ( pagination != null )
                return Paginate<T>( list, pagination.PageNumber, pagination.PageSize );
            return list;
        }
    }
}