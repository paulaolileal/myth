using AutoMapper;
using Myth.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Myth.ValueObjects.OdataObjects.Requests {

    public class FilterRequest<TSource, TDest> {
        public IEnumerable<string> Filter { get; set; } = new List<string>( );

        public FilterRequest( IEnumerable<string> filter ) {
            if ( filter != null )
                Filter = filter;
        }

        private Expression ConvertOperator( string @operator, Expression member, Expression value = null ) {
            return ( @operator.ToLower( ) ) switch
            {
                "eq" => Expression.Equal( member, value ),
                "ne" => Expression.NotEqual( member, value ),
                "gt" => Expression.GreaterThan( member, value ),
                "ge" => Expression.GreaterThanOrEqual( member, value ),
                "lt" => Expression.LessThan( member, value ),
                "le" => Expression.LessThanOrEqual( member, value ),
                "and" => Expression.And( member, value ),
                "or" => Expression.Or( member, value ),
                "not" => Expression.Not( member ),
                "add" => Expression.Add( member, value ),
                "sub" => Expression.Subtract( member, value ),
                "mul" => Expression.Multiply( member, value ),
                "div" => Expression.Divide( member, value ),
                "mod" => Expression.Modulo( member, value ),
                _ => throw new Exception( "Operator not exists!" ),
            };
        }

        public Expression<Func<TDest, bool>> Build( IMapper mapper ) {
            var parameter = Expression.Parameter( typeof( TSource ) );

            Expression expression = Expression.Constant( true );

            foreach ( var item in Filter.Where( x => !string.IsNullOrEmpty( x ) ) ) {
                var data = item.Split( " ", StringSplitOptions.RemoveEmptyEntries );

                MemberExpression property = ExpressionExtension.GenerateNavigationProperty<TSource>( data[ 0 ], parameter );

                Expression value = ExpressionExtension.GenerateConstant( data );

                if ( property.Type.Name.Contains( nameof( Nullable ) ) )
                    value = Expression.Convert( value, property.Type );

                var condition = ConvertOperator( data[ 1 ], property, value );

                var func = Expression.Lambda<Func<TSource, bool>>( condition, parameter ).Body;

                expression = Expression.And( expression, func );
            }

            var sourceExpression = Expression.Lambda<Func<TSource, bool>>( expression, parameter );

            var destExpression = mapper.Map<Expression<Func<TDest, bool>>>( sourceExpression );

            return destExpression;
        }
    }
}