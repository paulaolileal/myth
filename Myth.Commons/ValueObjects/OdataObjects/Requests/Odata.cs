using Myth.Extensions;
using Myth.ValueObjects.OdataObjects.Queries;
using System.Collections.Generic;

namespace Myth.ValueObjects.OdataObjects.Requests {

    public class Odata<TSource, TDest> {

        public IEnumerable<string> Filter { get; set; }

        public IEnumerable<string> Order { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public static implicit operator Odata<TDest>( Odata<TSource, TDest> odata ) {
            return odata.Build( );
        }
    }
}