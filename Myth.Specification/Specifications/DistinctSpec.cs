using Myth.Interfaces;
using System.Linq.Expressions;

namespace Myth.Specifications {

	public class DistinctSpec<T, TKey> : SpecBuilder<T> {
		private readonly ISpec<T> _left;

		private readonly Expression<Func<T, TKey>> _keySelector;

		public override Expression<Func<T, bool>> Predicate => _left.Predicate;

		public override Func<IQueryable<T>, IOrderedQueryable<T>> Sort => _left.Sort;

		public override Func<IQueryable<T>, IQueryable<T>> PostProcess => DistinctBy( _left, _keySelector );

		public DistinctSpec( ISpec<T> left, Expression<Func<T, TKey>> keySelector ) : base( left ) {
			_left = left;
			_keySelector = keySelector;
		}

		private Func<IQueryable<T>, IQueryable<T>> DistinctBy( ISpec<T> left, Expression<Func<T, TKey>> keySelector ) {
			Func<IQueryable<T>, IQueryable<T>> process;

			if ( left.PostProcess != null )
				process = items => left.PostProcess( items ).DistinctBy( keySelector );
			else
				process = items => items.DistinctBy( keySelector );

			return process;
		}
	}
}