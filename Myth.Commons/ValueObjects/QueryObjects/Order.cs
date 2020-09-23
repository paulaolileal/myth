using System;
using System.Linq.Expressions;

namespace Myth.ValueObjects.QueryObjects {

    public class Order<TDest> {
        public Expression<Func<TDest, object>> OrderBy { get; private set; }
        public bool Desc { get; private set; }

        public Order( Expression<Func<TDest, object>> order, bool desc = false ) {
            OrderBy = order;
            Desc = desc;
        }
    }
}