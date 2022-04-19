using Myth.Interfaces;
using Myth.Repositories;

namespace Myth.Extensions.Specifications {

    public static class OdataSpecificationExtension {

        public static ISpec<T> Odata<T>( this ISpec<T> spec, Odata<T> odata ) {
            if ( odata != null ) {
                if ( odata.Filter != null )
                    spec = spec.Filter( odata.Filter );

                if ( odata.Order != null )
                    foreach ( var order in odata.Order )
                        spec = spec.Order( order );

                if ( odata.Pagination != null )
                    spec = spec.Paginate( odata.Pagination );
            }

            return spec;
        }
    }
}