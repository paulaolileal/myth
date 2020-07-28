using AutoMapper;
using Myth.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Myth.ValueObjects.OdataObjects {

    public class OdataOrder<TSource, TDest> {
        public IEnumerable<string> Conditions { get; set; } = new List<string>( );

        public OdataOrder( IEnumerable<string> conditions ) {
            if ( conditions != null )
                Conditions = conditions;
        }

        public IEnumerable<OrderCondition<TDest>> Build( IMapper mapper ) {
            var parameter = Expression.Parameter( typeof( TSource ) );

            var conditions = new List<OrderCondition<TDest>>( );

            foreach ( var item in Conditions.Where( x => !string.IsNullOrEmpty( x ) ) ) {
                var fields = item.Split( " ", StringSplitOptions.RemoveEmptyEntries );

                var desc = false;
                if ( fields.Length > 1 && fields[ 1 ].ToLower( ) == "desc" )
                    desc = true;

                var property = ExpressionExtension.GenerateNavigationProperty<TSource>( fields[ 0 ], parameter );

                var sort = Expression.Lambda( property, parameter );

                var expression = mapper.Map<Expression<Func<TDest, object>>>( sort );

                var condition = new OrderCondition<TDest>( expression, desc );

                conditions.Add( condition );
            }

            return conditions;
        }
    }
}