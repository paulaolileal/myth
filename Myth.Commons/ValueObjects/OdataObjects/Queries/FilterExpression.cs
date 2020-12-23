using System;
using System.Linq.Expressions;

namespace Myth.ValueObjects.OdataObjects.Queries {

    public class FilterExpression<TDest> {
        public Expression<Func<TDest, bool>> Conditions { get; private set; }

        public FilterExpression( Expression<Func<TDest, bool>> conditions ) {
            Conditions = conditions;
        }
    }
}