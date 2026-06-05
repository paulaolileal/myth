using System.Linq.Expressions;
using Myth.Interfaces;

namespace Myth.Specifications;

public class IncludeSpec<T>( ISpec<T> left, Func<IQueryable<T>, IQueryable<T>> includeQuery ) : SpecBuilder<T>( left ) {
	private readonly ISpec<T> _left = left;

	private readonly Func<IQueryable<T>, IQueryable<T>> _includeQuery = includeQuery ?? throw new ArgumentNullException( nameof( includeQuery ) );

	public override Expression<Func<T, bool>> Predicate => _left.Predicate;

	public override Func<IQueryable<T>, IOrderedQueryable<T>> Sort => _left.Sort;

	public override Func<IQueryable<T>, IQueryable<T>> PostProcess => _left.PostProcess;

	public override Func<IQueryable<T>, IQueryable<T>>? Includes => Chain( _left, _includeQuery );

	private Func<IQueryable<T>, IQueryable<T>> Chain( ISpec<T> left, Func<IQueryable<T>, IQueryable<T>> includeQuery ) {
		Func<IQueryable<T>, IQueryable<T>> includes;

		if ( left.Includes != null )
			includes = items => includeQuery( left.Includes( items ) );
		else
			includes = items => includeQuery( items );

		return includes;
	}
}
