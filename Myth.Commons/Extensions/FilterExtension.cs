using System;
using System.Collections.Generic;
using System.Linq;

namespace Myth.Extensions {

    public static class FilterExtension {

        public static string Filter( this string url, string condition ) =>
             new String( url.Concat( $"{( url.Contains( "?" ) ? "&" : "?" )}Filter={condition}" ).ToArray( ) );

        public static string Filter( this string url, IEnumerable<string> filters ) {
            foreach ( var filter in filters )
                url.Filter( filter );

            return url;
        }
    }
}