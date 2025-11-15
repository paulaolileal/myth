using System.Linq.Expressions;

namespace Myth.Specifications;

internal class SwapVisitor( Expression from, Expression to ) : ExpressionVisitor {
	private readonly Expression _from = from;
	private readonly Expression _to = to;

	public override Expression? Visit( Expression? node ) => node == _from ? _to : base.Visit( node );
}
