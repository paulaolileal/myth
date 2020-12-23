using AutoMapper;
using Myth.ValueObjects.OdataObjects;
using Myth.ValueObjects.OdataObjects.Queries;
using Myth.ValueObjects.OdataObjects.Requests;

namespace Myth.Extensions {

    public static class OdataExtension {

        private static IMapper _mapper;

        public static void Configure( IMapper mapper ) {
            _mapper = mapper;
        }

        public static Odata<TDest> Build<TSource, TDest>( this Odata<TSource, TDest> odata ) {
            var order = new OrderRequest<TSource, TDest>( odata.Order ).Build( _mapper );
            var filter = new FilterRequest<TSource, TDest>( odata.Filter )?.Build( _mapper );
            var pagination = new Pagination( odata.PageNumber, odata.PageSize );

            return new Odata<TDest>( filter, order, pagination );
        }
    }
}