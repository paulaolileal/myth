using System.Linq.Expressions;

namespace Myth.Specifications {

	public class NullSpec<T> : SpecBuilder<T> {

		public override Expression<Func<T, bool>> Predicate =>
			Expression.Lambda<Func<T, bool>>(
				Expression.Constant( true ),
				Expression.Parameter( typeof( T ) ) );

		public override Func<IQueryable<T>, IOrderedQueryable<T>> Sort { get; }

		public override Func<IQueryable<T>, IQueryable<T>> PostProcess { get; }

		public NullSpec( ) {
			ItensSkiped = 0;
			ItemsTaked = 0;
		}
	}
}