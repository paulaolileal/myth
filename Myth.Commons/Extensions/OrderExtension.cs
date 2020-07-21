using System;
using System.Collections.Generic;
using System.Linq;

namespace Myth.Extensions {

    public static class OrderExtension {

        public static string Order( this string url, string condition ) =>
             new String( url.Concat( $"{( url.Contains( "?" ) ? "&" : "?" )}Order.Conditions={condition}" ).ToArray( ) );

        public static string Order( this string url, IEnumerable<string> orders ) {
            foreach ( var order in orders )
                url.Order( order );

            return url;
        }
    }
}