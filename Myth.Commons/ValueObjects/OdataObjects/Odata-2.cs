using AutoMapper;
using System.Collections.Generic;

namespace Myth.ValueObjects.OdataObjects {

    public class Odata<TSource, TDest> {
        public IEnumerable<string> Filter { get; set; }
        public IEnumerable<string> Order { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public Odata<TDest> Build( IMapper mapper ) {
            var order = new OdataOrder<TSource, TDest>( Order ).Build( mapper );
            var filter = new OdataFilter<TSource, TDest>( Filter )?.Build( mapper );
            var pagination = new Pagination( PageNumber, PageSize );

            return new Odata<TDest>( filter, order, pagination );
        }
    }
}