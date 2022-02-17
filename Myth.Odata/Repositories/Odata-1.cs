using Myth.Repositories.Expressions;
using Myth.ValueObjects;
using System.Linq.Expressions;

namespace Myth.Repositories {

    public class Odata<TDest> {
        public IEnumerable<OrderExpression<TDest>> Order { get; set; }

        public FilterExpression<TDest> Filter { get; set; }

        public Pagination Pagination { get; set; }

        public static Odata<TDest> Default => new( pagination: Pagination.All );

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