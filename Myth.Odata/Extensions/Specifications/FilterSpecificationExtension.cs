using Myth.Interfaces;
using Myth.Repositories.Expressions;

namespace Myth.Extensions.Specifications {

    public static class FilterSpecificationExtension {

        public static ISpec<T> Filter<T>( this ISpec<T> spec, FilterExpression<T> filter ) {
            if ( filter.Conditions != null )
                spec = spec.And( filter.Conditions );

            return spec;
        }
    }
}