using Myth.ValueObjects.QueryObjects;
using System;
using System.Linq;

namespace Myth.Extensions {

    public static class PaginateExtension {

        [Obsolete]
        public static string Paginate( this string url, int startIndex, int pageSize ) =>
             new String( url.Concat( $"{( url.Contains( "?" ) ? "&" : "?" )}PageNumber={startIndex}&PageSize={pageSize}" ).ToArray( ) );

        [Obsolete]
        public static string Paginate( this string url, Pagination pagination ) =>
             url.Paginate( pagination.PageNumber, pagination.PageSize );
    }
}