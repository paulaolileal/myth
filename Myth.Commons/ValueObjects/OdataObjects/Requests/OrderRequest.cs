using AutoMapper;
using Myth.Extensions;
using Myth.ValueObjects.OdataObjects.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Myth.ValueObjects.OdataObjects.Requests {

    public class OrderRequest<TSource, TDest> {

        public IEnumerable<string> Order { get; set; } = new List<string>( );

        public OrderRequest( IEnumerable<string> order ) {
            if ( order != null )
                Order = order;
        }

        public IEnumerable<ConditionExpression<TDest>> Build( IMapper mapper ) {
            var parameter = Expression.Parameter( typeof( TSource ) );

            var conditions = new List<ConditionExpression<TDest>>( );

            foreach ( var item in Order.Where( x => !string.IsNullOrEmpty( x ) ) ) {
                var fields = item.Split( " ", StringSplitOptions.RemoveEmptyEntries );

                var desc = false;
                if ( fields.Length > 1 && fields[ 1 ].ToLower( ) == "desc" )
                    desc = true;

                var property = ExpressionExtension.GenerateNavigationProperty<TSource>( fields[ 0 ], parameter );

                var sort = Expression.Lambda( property, parameter );

                var expression = mapper.Map<Expression<Func<TDest, object>>>( sort );

                var condition = new ConditionExpression<TDest>( expression, desc );

                conditions.Add( condition );
            }

            return conditions;
        }
    }
}