using System.Linq.Expressions;

namespace Myth.Specifications {

    public class ExpressionSpec<T> : SpecBuilder<T> {
        private readonly Expression<Func<T, bool>> _predicate;

        public override Expression<Func<T, bool>> Predicate => _predicate;

        public override Func<IQueryable<T>, IOrderedQueryable<T>> Sort { get; }

        public override Func<IQueryable<T>, IQueryable<T>> PostProcess { get; }

        public ExpressionSpec( Expression<Func<T, bool>> predicate ) {
            _predicate = predicate;
        }
    }
}