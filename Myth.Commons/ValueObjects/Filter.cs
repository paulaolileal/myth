using System;
using System.Linq.Expressions;

namespace Myth.ValueObjects {
    public class Filter<TDest> {
        public Expression<Func<TDest, bool>> Conditions { get; private set; }

        public Filter( Expression<Func<TDest, bool>> conditions ) {
            Conditions = conditions;
        }
    }
}