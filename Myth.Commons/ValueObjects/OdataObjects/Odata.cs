using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Myth.ValueObjects.OdataObjects {

    public class Odata<TDest> {
        public IEnumerable<Order<TDest>> Order { get; set; }
        public Filter<TDest> Filter { get; private set; }
        public Pagination Pagination { get; private set; }

        public Odata( Expression<Func<TDest, bool>> filter, IEnumerable<OrderCondition<TDest>> orders, Pagination pagination ) {
            if ( filter != null )
                Filter = new Filter<TDest>( filter );

            if ( orders != null )
                Order = orders.Select( order => new Order<TDest>( order.Expression, order.Descending ) );

            if ( pagination != null )
                Pagination = pagination;
        }
    }
}