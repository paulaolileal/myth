using System.Collections.Generic;
using System.Linq;

namespace Myth.Extensions {

    public static class ListExtension {

        public static string ToStringWithSeparator( this IEnumerable<string> list, string separator = ", " ) {
            if ( list != null )
                return string.Join( separator, list.ToArray( ) );
            return string.Empty;
        }
    }
}