using System;
using System.Linq.Expressions;

namespace Myth.ValueObjects.OdataObjects.Queries {

    public class OrderExpression<TDest> {

        public Expression<Func<TDest, object>> OrderBy { get; private set; }

        public bool Desc { get; private set; }

        public OrderExpression( Expression<Func<TDest, object>> order, bool desc = false ) {
            OrderBy = order;
            Desc = desc;
        }
    }
}