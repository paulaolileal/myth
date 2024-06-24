using Myth.Interfaces;
using System.Linq.Expressions;

namespace Myth.Specifications {

	public class NotSpec<T> : SpecBuilder<T> {
		private readonly ISpec<T> _left;

		public override Expression<Func<T, bool>> Predicate => Not( _left.Predicate );

		public override Func<IQueryable<T>, IOrderedQueryable<T>> Sort => _left.Sort;

		public override Func<IQueryable<T>, IQueryable<T>> PostProcess => _left.PostProcess;

		public NotSpec( ISpec<T> left ) : base( left ) {
			_left = left ?? throw new ArgumentNullException( nameof( left ) );
		}

		private static Expression<Func<T, bool>> Not( Expression<Func<T, bool>> left ) {
			ArgumentNullException.ThrowIfNull( left );

			var notExpression = Expression.Not( left.Body );
			var lambda = Expression.Lambda<Func<T, bool>>( notExpression, left.Parameters.Single( ) );
			return lambda;
		}
	}
}