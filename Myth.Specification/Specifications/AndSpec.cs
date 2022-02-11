using Myth.Interfaces;
using System.Linq.Expressions;

namespace Myth.Specifications {

    public class AndSpec<T> : SpecBuilder<T> {
        private readonly ISpec<T> _left;

        private readonly ISpec<T> _right;

        public override Func<IQueryable<T>, IOrderedQueryable<T>> Sort => _left.Sort;

        public override Func<IQueryable<T>, IQueryable<T>> PostProcess => _left.PostProcess;

        public override Expression<Func<T, bool>> Predicate =>
            _left.Predicate != null ? And( _left.Predicate, _right.Predicate ) : _right.Predicate;

        public AndSpec( ISpec<T> left, ISpec<T> right ) : base( left ) {
            _left = left;
            _right = right ?? throw new ArgumentNullException( nameof( right ) );
        }

        private static Expression<Func<T, bool>> And( Expression<Func<T, bool>> left, Expression<Func<T, bool>> right ) {
            if ( left == null )
                throw new ArgumentNullException( nameof( left ) );

            if ( right == null )
                throw new ArgumentNullException( nameof( right ) );

            var visitor = new SwapVisitor( left.Parameters[ 0 ], right.Parameters[ 0 ] );
            var binaryExpression = Expression.AndAlso( visitor.Visit( left.Body ), right.Body );
            var lambda = Expression.Lambda<Func<T, bool>>( binaryExpression, right.Parameters );
            return lambda;
        }
    }
}