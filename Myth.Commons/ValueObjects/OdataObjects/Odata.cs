using AutoMapper;

namespace Myth.ValueObjects.OdataObjects {

    public class Odata<TSource, TDest> {
        public OdataFilter<TSource, TDest> Filter { get; set; }
        public OdataOrder<TSource, TDest> Order { get; set; }
        public Pagination Pagination { get; set; }

        public OdataCast<TDest> Build( IMapper mapper ) {
            var order = Order?.Build( mapper );
            var filter = Filter?.Build( mapper );

            return new OdataCast<TDest>( filter, order, Pagination );
        }
    }
}