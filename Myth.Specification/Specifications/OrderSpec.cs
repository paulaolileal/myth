using Myth.Interfaces;
using System.Linq.Expressions;

namespace Myth.Specifications {

    public class OrderSpec<T, TProperty> : SpecBuilder<T> {
        private readonly ISpec<T> _left;

        private readonly Expression<Func<T, TProperty>> _property;

        public override Expression<Func<T, bool>> Predicate => _left.Predicate;

        public override Func<IQueryable<T>, IOrderedQueryable<T>> Sort => Order( _left, _property );

        public override Func<IQueryable<T>, IQueryable<T>> PostProcess => _left.PostProcess;

        public OrderSpec( ISpec<T> left, Expression<Func<T, TProperty>> property ) : base( left ) {
            _left = left;
            _property = property;
        }

        private Func<IQueryable<T>, IOrderedQueryable<T>> Order( ISpec<T> left, Expression<Func<T, TProperty>> property ) {
            Func<IQueryable<T>, IOrderedQueryable<T>> sort;
            if ( left.Sort != null )
                sort = items => left.Sort( items ).ThenBy( property );
            else
                sort = items => items.OrderBy( property );

            return sort;
        }
    }
}