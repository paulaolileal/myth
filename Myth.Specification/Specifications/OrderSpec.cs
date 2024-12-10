using Myth.Interfaces;
using System.Linq.Expressions;

namespace Myth.Specifications;

public class OrderSpec<T, TProperty>( ISpec<T> left, Expression<Func<T, TProperty>> property ) : SpecBuilder<T>( left ) {
	private readonly ISpec<T> _left = left;

	private readonly Expression<Func<T, TProperty>> _property = property;

	public override Expression<Func<T, bool>> Predicate => _left.Predicate;

	public override Func<IQueryable<T>, IOrderedQueryable<T>> Sort => Order( _left, _property );

	public override Func<IQueryable<T>, IQueryable<T>> PostProcess => _left.PostProcess;

	private Func<IQueryable<T>, IOrderedQueryable<T>> Order( ISpec<T> left, Expression<Func<T, TProperty>> property ) {
		Func<IQueryable<T>, IOrderedQueryable<T>> sort;

		if ( left.Sort != null )
			sort = items => left.Sort( items ).ThenBy( property );
		else
			sort = items => items.OrderBy( property );

		return sort;
	}
}