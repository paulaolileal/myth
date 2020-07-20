using Myth.ValueObjects;
using System;
using System.Linq;

namespace Myth.Extensions {

    public static class PaginateExtension {

        public static string Paginate( this string url, int startIndex, int pageSize ) =>
             new String( url.Concat( $"{( url.Contains( "?" ) ? "&" : "?" )}PageNumber={startIndex}&PageSize={pageSize}" ).ToArray( ) );

        public static string Paginate( this string url, Pagination pagination ) =>
             url.Paginate( pagination.PageNumber, pagination.PageSize );
    }
}