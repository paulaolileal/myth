using Myth.Interfaces;
using Myth.ValueObjects;
using System.Linq.Expressions;

namespace Myth.Specifications;

/// <summary>
/// Specification that applies pagination using Skip and Take operations based on a Pagination value object
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public class PaginationSpec<T> : SpecBuilder<T> {
	private readonly ISpec<T> _left;

	private readonly Pagination _pagination;

	public override Expression<Func<T, bool>> Predicate => _left.Predicate;

	public override Func<IQueryable<T>, IOrderedQueryable<T>> Sort => _left.Sort;

	public override Func<IQueryable<T>, IQueryable<T>> PostProcess => ApplyPagination( _left, _pagination );

	/// <summary>
	/// Creates a new pagination specification
	/// </summary>
	/// <param name="left">The previous specification in the chain</param>
	/// <param name="pagination">The pagination parameters</param>
	public PaginationSpec( ISpec<T> left, Pagination pagination ) : base( left ) {
		_left = left;
		_pagination = pagination;

		// Calculate skip amount for state tracking
		var pageNumber = Math.Max( pagination.PageNumber, 1 );
		var pageSize = Math.Max( pagination.PageSize, 0 );

		// Only update state if not using Pagination.All
		if ( pagination != Pagination.All && pageSize > 0 ) {
			var skipAmount = ( pageNumber - 1 ) * pageSize;
			ItemsSkiped += skipAmount;
			ItemsTaked += pageSize;
		}
	}

	private Func<IQueryable<T>, IQueryable<T>> ApplyPagination( ISpec<T> left, Pagination pagination ) {
		Func<IQueryable<T>, IQueryable<T>> process;

		// Handle special case: Pagination.All means no pagination
		if ( pagination == Pagination.All ) {
			process = left.PostProcess ?? ( items => items );
		} else {
			var pageNumber = Math.Max( pagination.PageNumber, 1 );
			var pageSize = pagination.PageSize;

			// Only apply pagination if pageSize > 0
			if ( pageSize > 0 ) {
				var skipAmount = ( pageNumber - 1 ) * pageSize;

				if ( left.PostProcess != null )
					process = items => left.PostProcess( items ).Skip( skipAmount ).Take( pageSize );
				else
					process = items => items.Skip( skipAmount ).Take( pageSize );
			} else {
				// If pageSize <= 0, don't apply pagination
				process = left.PostProcess ?? ( items => items );
			}
		}

		return process;
	}
}