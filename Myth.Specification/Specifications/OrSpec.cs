using System.Linq.Expressions;
using Myth.Interfaces;

namespace Myth.Specifications;

public class OrSpec<T>( ISpec<T> left, ISpec<T> right ) : SpecBuilder<T>( left ) {
	private readonly ISpec<T> _left = left;

	private readonly ISpec<T> _right = right ?? throw new ArgumentNullException( nameof( right ) );

	public override Expression<Func<T, bool>> Predicate =>
						_left.Predicate != null ? Or( _left.Predicate, _right.Predicate ) : _right.Predicate;

	public override Func<IQueryable<T>, IOrderedQueryable<T>> Sort => _left.Sort;

	public override Func<IQueryable<T>, IQueryable<T>> PostProcess => _left.PostProcess;

	private static Expression<Func<T, bool>> Or( Expression<Func<T, bool>> left, Expression<Func<T, bool>> right ) {
		ArgumentNullException.ThrowIfNull( left );

		ArgumentNullException.ThrowIfNull( right );

		var visitor = new SwapVisitor( left.Parameters[ 0 ], right.Parameters[ 0 ] );
		var binaryExpression = Expression.OrElse( visitor.Visit( left.Body )!, right.Body );
		var lambda = Expression.Lambda<Func<T, bool>>( binaryExpression, right.Parameters );
		return lambda;
	}
}
