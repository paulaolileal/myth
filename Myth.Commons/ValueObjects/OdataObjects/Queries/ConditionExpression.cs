using System;
using System.Linq.Expressions;

namespace Myth.ValueObjects.OdataObjects.Queries {

    public class ConditionExpression<T> {

        public Expression<Func<T, object>> Expression { get; private set; }

        public bool Descending { get; private set; }

        public ConditionExpression( Expression<Func<T, object>> expression, bool descending ) {
            Expression = expression;
            Descending = descending;
        }

        public ConditionExpression( Expression<Func<T, object>> expression )
            : this( expression, false ) {
        }
    }
}