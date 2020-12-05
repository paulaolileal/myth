using System;
using System.Linq.Expressions;

namespace Myth.ValueObjects.OdataObjects {

    public class OrderCondition<T> {
        public Expression<Func<T, object>> Expression { get; private set; }

        public bool Descending { get; private set; }

        public OrderCondition( Expression<Func<T, object>> expression, bool descending ) {
            Expression = expression;
            Descending = descending;
        }

        public OrderCondition( Expression<Func<T, object>> expression )
            : this( expression, false ) {
        }
    }
}