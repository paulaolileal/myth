using Myth.Interfaces;
using System.Linq.Expressions;

namespace Myth.Specifications;

public class AndSpec<T>( ISpec<T> left, ISpec<T> right ) : SpecBuilder<T>( left ) {
	private readonly ISpec<T> _left = left;

	private readonly ISpec<T> _right = right ?? throw new ArgumentNullException( nameof( right ) );

	public override Func<IQueryable<T>, IOrderedQueryable<T>> Sort => _left.Sort;

	public override Func<IQueryable<T>, IQueryable<T>> PostProcess => _left.PostProcess;

	public override Expression<Func<T, bool>> Predicate =>
		_left.Predicate != null ? And( _left.Predicate, _right.Predicate ) : _right.Predicate;

	private static Expression<Func<T, bool>> And( Expression<Func<T, bool>> left, Expression<Func<T, bool>> right ) {
		ArgumentNullException.ThrowIfNull( left );

		ArgumentNullException.ThrowIfNull( right );

		var visitor = new SwapVisitor( left.Parameters[ 0 ], right.Parameters[ 0 ] );
		var binaryExpression = Expression.AndAlso( visitor.Visit( left.Body )!, right.Body );
		var lambda = Expression.Lambda<Func<T, bool>>( binaryExpression, right.Parameters );
		return lambda;
	}
}