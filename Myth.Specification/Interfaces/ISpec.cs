using System.Linq.Expressions;

namespace Myth.Interfaces {

    public interface ISpec<T> {
        Expression<Func<T, bool>> Predicate { get; }

        Func<IQueryable<T>, IOrderedQueryable<T>> Sort { get; }

        Func<IQueryable<T>, IQueryable<T>> PostProcess { get; }

        int ItensSkiped { get; }

        int ItensTaked { get; }

        bool IsSatisfiedBy( T entity );

        IQueryable<T> Prepare( IQueryable<T> query );

        IQueryable<T> Filtered( IQueryable<T> query );

        IQueryable<T> Sorted( IQueryable<T> query );

        IQueryable<T> Processed( IQueryable<T> query );

        T? SatisfyingItemFrom( IQueryable<T> query );

        IQueryable<T> SatisfyingItemsFrom( IQueryable<T> query );

        ISpec<T> InitEmpty( );

        ISpec<T> And( ISpec<T> specification );

        ISpec<T> And( Expression<Func<T, bool>> right );

        ISpec<T> Or( ISpec<T> specification );

        ISpec<T> Or( Expression<Func<T, bool>> right );

        ISpec<T> Not( );

        ISpec<T> Skip( int amount );

        ISpec<T> Take( int amount );

        ISpec<T> Order<TProperty>( Expression<Func<T, TProperty>> property );

        ISpec<T> OrderDescending<TProperty>( Expression<Func<T, TProperty>> property );

        ISpec<T> DistinctBy<TProperty>( Expression<Func<T, TProperty>> property );
    }
}