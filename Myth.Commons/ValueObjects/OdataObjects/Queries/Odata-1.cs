using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Myth.ValueObjects.OdataObjects.Queries {

    public class Odata<TDest> {

        public IEnumerable<OrderExpression<TDest>> Order { get; set; }

        public FilterExpression<TDest> Filter { get; private set; }

        public Pagination Pagination { get; private set; }

        public static Odata<TDest> Default => new Odata<TDest>( pagination: Pagination.All );

        public Odata( Expression<Func<TDest, bool>> filter = null, IEnumerable<ConditionExpression<TDest>> orders = null, Pagination pagination = null ) {
            if ( filter != null )
                Filter = new FilterExpression<TDest>( filter );

            if ( orders != null )
                Order = orders.Select( order => new OrderExpression<TDest>( order.Expression, order.Descending ) );

            if ( pagination != null )
                Pagination = pagination;
        }
    }
}