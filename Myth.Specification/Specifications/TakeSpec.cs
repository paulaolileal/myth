using Myth.Interfaces;
using System.Linq.Expressions;

namespace Myth.Specifications;

public class TakeSpec<T> : SpecBuilder<T> {
	private readonly ISpec<T> _left;

	private readonly int _amount;

	public override Expression<Func<T, bool>> Predicate => _left.Predicate;

	public override Func<IQueryable<T>, IOrderedQueryable<T>> Sort => _left.Sort;

	public override Func<IQueryable<T>, IQueryable<T>> PostProcess => Take( _left, _amount );

	public TakeSpec( ISpec<T> left, int amount ) : base( left ) {
		_left = left;
		_amount = amount;
		ItemsTaked += amount;
	}

	private Func<IQueryable<T>, IQueryable<T>> Take( ISpec<T> left, int amount ) {
		Func<IQueryable<T>, IQueryable<T>> process;

		if ( left.PostProcess != null )
			process = items => left.PostProcess( items ).Take( amount );
		else
			process = items => items.Take( amount );

		return process;
	}
}