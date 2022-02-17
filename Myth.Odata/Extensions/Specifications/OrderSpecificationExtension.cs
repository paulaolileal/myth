using Myth.Interfaces;
using Myth.Repositories.Expressions;

namespace Myth.Extensions.Specifications
{

    public static class OrderSpecificationExtension {

        public static ISpec<T> Order<T>( this ISpec<T> spec, OrderExpression<T> filter ) {
            if ( filter.OrderBy != null ) {
                if ( filter.Desc )
                    spec = spec.OrderDescending( filter.OrderBy );
                else
                    spec = spec.Order( filter.OrderBy );
            }

            return spec;
        }
    }
}
