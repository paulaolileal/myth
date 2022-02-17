using AutoMapper;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.Parser;
using System.Linq.Expressions;

namespace Myth.Api.Requests {

    public class FilterRequest<TSource, TDest> {
        public string Filter { get; set; }

        public FilterRequest( string filter ) {
            Filter = filter;
        }

        public Expression<Func<TDest, bool>> Build( IMapper mapper ) {
            var parameter = Expression.Parameter( typeof( TSource ) );

            Expression expression = Expression.Constant( true );

            if ( !string.IsNullOrEmpty( Filter ) )
                expression = new ExpressionParser(
                    new[ ] { parameter },
                    Filter,
                    Array.Empty<object>( ),
                    new ParsingConfig( )
                ).Parse( typeof( bool ) );

            var sourceExpression = Expression.Lambda<Func<TSource, bool>>( expression, parameter );

            var destExpression = mapper.Map<Expression<Func<TDest, bool>>>( sourceExpression );

            return destExpression;
        }
    }
}