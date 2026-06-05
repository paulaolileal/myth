using System.Linq.Expressions;
using Myth.ValueObjects;

namespace Myth.Interfaces;

public interface ISpec<T> {

	/// <summary>
	/// The predicate
	/// </summary>
	Expression<Func<T, bool>> Predicate { get; }

	/// <summary>
	/// Amount of items jumped
	/// </summary>
	int ItemsSkiped { get; }

	/// <summary>
	/// Amount of items taked
	/// </summary>
	int ItemsTaked { get; }

	/// <summary>
	/// The predicated builded on a query
	/// </summary>
	Func<T, bool> Query { get; }

	/// <summary>
	/// Checks if a entity is statisfyed by specification
	/// </summary>
	/// <param name="entity"></param>
	/// <returns>A value that represents the answer</returns>
	bool IsSatisfiedBy( T entity );

	/// <summary>
	/// Sort function aggregation
	/// </summary>
	Func<IQueryable<T>, IOrderedQueryable<T>> Sort { get; }

	/// <summary>
	/// Pagination and pos processing function aggregation
	/// </summary>
	Func<IQueryable<T>, IQueryable<T>> PostProcess { get; }

	/// <summary>
	/// Include (eager loading) function aggregation
	/// </summary>
	Func<IQueryable<T>, IQueryable<T>>? Includes { get; }

	/// <summary>
	/// Runs all function aggregation
	/// </summary>
	/// <param name="query">The collection</param>
	/// <returns>A queryable collection</returns>
	IQueryable<T> Prepare( IQueryable<T> query );

	/// <summary>
	/// Add a filter function aggregation
	/// </summary>
	/// <param name="query">The collection</param>
	/// <returns>A queryable collection</returns>
	IQueryable<T> Filtered( IQueryable<T> query );

	/// <summary>
	/// Add sort function aggregation
	/// </summary>
	/// <param name="query">The collection</param>
	/// <returns>A queryable collection</returns>
	IQueryable<T> Sorted( IQueryable<T> query );

	/// <summary>
	/// Add a post processing function aggregation
	/// </summary>
	/// <param name="query">The collection</param>
	/// <returns>A queryable collection</returns>
	IQueryable<T> Processed( IQueryable<T> query );

	/// <summary>
	/// Add an include (eager loading) function aggregation
	/// </summary>
	/// <param name="query">The collection</param>
	/// <returns>A queryable collection</returns>
	IQueryable<T> Included( IQueryable<T> query );

	/// <summary>
	/// Get a element that is satisfyed by specifications from a list
	/// </summary>
	/// <param name="query">The collection</param>
	/// <returns>A queryable collection</returns>
	T? SatisfyingItemFrom( IQueryable<T> query );

	/// <summary>
	/// Get a list of elements that are satisfyed by specifications from list
	/// </summary>
	/// <param name="query">The collection</param>
	/// <returns>A queryable collection</returns>
	IQueryable<T> SatisfyingItemsFrom( IQueryable<T> query );

	/// <summary>
	/// Add a `EMPTY` specification
	/// </summary>
	/// <returns>This object</returns>
	ISpec<T> InitEmpty( );

	/// <summary>
	/// Add a `AND` specification
	/// </summary>
	/// <param name="specification">The specification</param>
	/// <returns>This object</returns>
	ISpec<T> And( ISpec<T> specification );

	/// <summary>
	/// Add a `ÀND` specification
	/// </summary>
	/// <param name="right">The expression</param>
	/// <returns>This object</returns>
	ISpec<T> And( Expression<Func<T, bool>> right );

	/// <summary>
	/// Add a `AND` specification if condition is `true`
	/// </summary>
	/// <param name="condition">The condition</param>
	/// <param name="other">the specification</param>
	/// <returns>This object</returns>
	ISpec<T> AndIf( bool condition, ISpec<T> other );

	/// <summary>
	/// Add a `AND` specification if condition is `true`
	/// </summary>
	/// <param name="condition">The condition</param>
	/// <param name="other">The expression</param>
	/// <returns>This object</returns>
	ISpec<T> AndIf( bool condition, Expression<Func<T, bool>> other );

	/// <summary>
	/// Add a `OR` specification
	/// </summary>
	/// <param name="specification">The specification</param>
	/// <returns>This object</returns>
	ISpec<T> Or( ISpec<T> specification );

	/// <summary>
	/// Add a `OR` specification
	/// </summary>
	/// <param name="right">The predicate</param>
	/// <returns>This object</returns>
	ISpec<T> Or( Expression<Func<T, bool>> right );

	/// <summary>
	/// Add a `OR` specification if condition is `true`
	/// </summary>
	/// <param name="condition">The condition</param>
	/// <param name="other">The specification</param>
	/// <returns>This object</returns>
	ISpec<T> OrIf( bool condition, ISpec<T> other );

	/// <summary>
	/// Add a `OR` specification if condition is `true`
	/// </summary>
	/// <param name="condition">The condition</param>
	/// <param name="other">The predicate</param>
	/// <returns>This object</returns>
	ISpec<T> OrIf( bool condition, Expression<Func<T, bool>> other );

	/// <summary>
	/// Add a `NOT` specification
	/// </summary>
	/// <returns>This object</returns>
	ISpec<T> Not( );

	/// <summary>
	/// Add the number of elements to skip in pagination
	/// </summary>
	/// <param name="amount">Number of elements</param>
	/// <returns>This object</returns>
	ISpec<T> Skip( int amount );

	/// <summary>
	/// Add the number of elements to take in pagination
	/// </summary>
	/// <param name="amount">Number of elements</param>
	/// <returns>This object</returns>
	ISpec<T> Take( int amount );

	/// <summary>
	/// Apply pagination using the provided Pagination value object
	/// </summary>
	/// <param name="pagination">The pagination parameters containing PageNumber and PageSize</param>
	/// <returns>This object</returns>
	ISpec<T> WithPagination( Pagination pagination );

	/// <summary>
	/// Add a sort specification in ascending order
	/// </summary>
	/// <typeparam name="TProperty">Type of property to sort</typeparam>
	/// <param name="property">Property to sort</param>
	/// <returns>This object</returns>
	ISpec<T> Order<TProperty>( Expression<Func<T, TProperty>> property );

	/// <summary>
	/// Add a sort specification in descending order
	/// </summary>
	/// <typeparam name="TProperty">Type of property to sort</typeparam>
	/// <param name="property">Property to sort</param>
	/// <returns>This object</returns>
	ISpec<T> OrderDescending<TProperty>( Expression<Func<T, TProperty>> property );

	/// <summary>
	/// Add a distinct specification
	/// </summary>
	/// <typeparam name="TProperty">Type of property to distinct</typeparam>
	/// <param name="property">Property to distinct</param>
	/// <returns>This object</returns>
	ISpec<T> DistinctBy<TProperty>( Expression<Func<T, TProperty>> property );

	/// <summary>
	/// Add an include (eager loading) specification for navigation properties
	/// </summary>
	/// <param name="includeQuery">Function that applies Include/ThenInclude on the queryable</param>
	/// <returns>This object</returns>
	ISpec<T> Include( Func<IQueryable<T>, IQueryable<T>> includeQuery );
}
