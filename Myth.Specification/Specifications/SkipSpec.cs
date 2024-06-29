using Myth.Interfaces;
using System.Linq.Expressions;

namespace Myth.Specifications;

public class SkipSpec<T> : SpecBuilder<T> {
	private readonly ISpec<T> _left;

	private readonly int _amount;

	public override Expression<Func<T, bool>> Predicate => _left.Predicate;

	public override Func<IQueryable<T>, IOrderedQueryable<T>> Sort => _left.Sort;

	public override Func<IQueryable<T>, IQueryable<T>> PostProcess => Skip( _left, _amount );

	public SkipSpec( ISpec<T> left, int amount ) : base( left ) {
		_left = left;

		_amount = amount;

		ItemsSkiped += amount;
	}

	private Func<IQueryable<T>, IQueryable<T>> Skip( ISpec<T> left, int amount ) {
		Func<IQueryable<T>, IQueryable<T>> process;

		if ( left.PostProcess != null )
			process = items => left.PostProcess( items ).Skip( amount );
		else
			process = items => items.Skip( amount );

		return process;
	}
}