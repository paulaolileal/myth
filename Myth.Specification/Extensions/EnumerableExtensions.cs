using Myth.Interfaces;

namespace Myth.Extensions;

public static class EnumerableExtensions {

	/// <summary>
	/// Filter sequence of values based on predicate
	/// </summary>
	/// <param name="spec">Predicate based on specification</param>
	/// <param name="values">The list of values</param>
	/// <returns>An queryable collection that satisfy the predicate</returns>
	public static IEnumerable<T> Where<T>( this IEnumerable<T> values, ISpec<T> spec ) => values.Where( spec.Query );

	/// <summary>
	/// Filter the sequence of values based on predicate
	/// </summary>
	/// <param name="values">The list of values</param>
	/// <param name="spec">Predicate based on specification</param>
	/// <returns>An queryable collection that satisfy the predicate</returns>
	public static IQueryable<T> Specify<T>( this IEnumerable<T> values, ISpec<T> spec ) => spec.Prepare( values.AsQueryable( ) );

	/// <summary>
	/// Filter the sequence of values based on predicate
	/// </summary>
	/// <param name="values">The list of values</param>
	/// <param name="spec">Predicate based on specification</param>
	/// <returns>An queryable collection that satisfy the predicate</returns>
	public static IQueryable<T> Filter<T>( this IQueryable<T> values, ISpec<T> spec ) => values.Where( spec.Predicate );

	/// <summary>
	/// Sorts a sequence of values based on predicate
	/// </summary>
	/// <param name="values">The list of values</param>
	/// <param name="spec">Predicate based on specification</param>
	/// <returns>An queryable collection that satisfy the predicate</returns>
	public static IQueryable<T> Sort<T>( this IQueryable<T> values, ISpec<T> spec ) => spec.Sort( values );

	/// <summary>
	/// Paginates a sequence of values based on predicate
	/// </summary>
	/// <param name="values">The list of values</param>
	/// <param name="spec">Predicate based on specification</param>
	/// <returns>An queryable collection that satisfy the predicate</returns>
	public static IQueryable<T> Paginate<T>( this IQueryable<T> values, ISpec<T> spec ) => spec.PostProcess( values );
}