using AutoMapper;
using Myth.Extensions;
using Myth.ValueObjects.OdataObjects.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace Myth.ValueObjects.OdataObjects.Requests {

    public class OrderRequest<TSource, TDest> {

        public string Order { get; set; }

        public OrderRequest( string order ) {
            Order = order;
        }

        public IEnumerable<ConditionExpression<TDest>> Build( IMapper mapper ) {
            var parameter = Expression.Parameter( typeof( TSource ) );

            var conditions = new List<ConditionExpression<TDest>>( );

            var itens = Order.Split( ",", StringSplitOptions.RemoveEmptyEntries );

            foreach ( var item in itens ) {
                var clause = item.Split( " ", StringSplitOptions.RemoveEmptyEntries );

                var desc = clause.Length > 1 && clause.ElementAt( 1 ).ToLower() == "desc";

                var sort = DynamicExpressionParser.ParseLambda( new ParameterExpression[ ] { parameter }, null, clause.First() );

                var expression = mapper.Map<Expression<Func<TDest, object>>>( sort );

                var condition = new ConditionExpression<TDest>( expression, desc );

                conditions.Add( condition );
            }


            return conditions;
        }
    }
}