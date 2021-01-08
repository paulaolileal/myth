using Microsoft.AspNetCore.Mvc;
using Myth.Extensions;
using Myth.ValueObjects.OdataObjects.Queries;
using System.Collections.Generic;

namespace Myth.ValueObjects.OdataObjects.Requests {

    public class Odata<TSource, TDest> {

        [FromQuery(Name = "$filter")]
        public string Filter { get; set; }

        [FromQuery(Name = "$orderby")]
        public string Order { get; set; }

        [FromQuery(Name = "$pagenumber")]
        public int PageNumber { get; set; }

        [FromQuery( Name = "$pagesize" )]
        public int PageSize { get; set; }

        public Odata( ) {
            Filter = Order =  string.Empty;
            PageNumber = PageSize = 0;
        }

        public static implicit operator Odata<TDest>( Odata<TSource, TDest> odata ) => odata.Build( );
    }
}