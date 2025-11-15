using System.Linq.Expressions;

namespace Myth.Specifications;

public class ExpressionSpec<T>( Expression<Func<T, bool>> predicate ) : SpecBuilder<T> {
	private readonly Expression<Func<T, bool>> _predicate = predicate;

	public override Expression<Func<T, bool>> Predicate => _predicate;

	public override Func<IQueryable<T>, IOrderedQueryable<T>> Sort { get; }

	public override Func<IQueryable<T>, IQueryable<T>> PostProcess { get; }
}
