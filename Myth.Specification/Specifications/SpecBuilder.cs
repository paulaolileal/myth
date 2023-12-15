using Myth.Exceptions;
using Myth.Interfaces;
using System.Linq.Expressions;

namespace Myth.Specifications {

    public abstract class SpecBuilder<T> : ISpec<T> {
        public int ItensSkiped { get; set; }
        public int ItensTaked { get; set; }
        public abstract Func<IQueryable<T>, IQueryable<T>> PostProcess { get; }
        public abstract Expression<Func<T, bool>> Predicate { get; }
        public abstract Func<IQueryable<T>, IOrderedQueryable<T>> Sort { get; }
        public Func<T, bool> Query => Predicate.Compile( );

        protected SpecBuilder( ) {
        }

        protected SpecBuilder( ISpec<T> left ) {
            ItensSkiped = left.ItensSkiped;
            ItensTaked = left.ItensTaked;
        }

        public static ISpec<T> Create( ) => new NullSpec<T>( );

        public IQueryable<T> SatisfyingItemsFrom( IQueryable<T> query ) => Prepare( query );

        public T? SatisfyingItemFrom( IQueryable<T> query ) {
            if ( query is null )
                throw new ArgumentNullException( nameof( query ) );

            return Prepare( query ).SingleOrDefault( );
        }

        public ISpec<T> And( ISpec<T> other ) => new AndSpec<T>( this, other );

        public ISpec<T> And( Expression<Func<T, bool>> other ) {
            if ( other == null )
                throw new ArgumentNullException( nameof( other ) );

            return new AndSpec<T>( this, new ExpressionSpec<T>( other ) );
        }

        public IQueryable<T> Filtered( IQueryable<T> query ) {
            if ( Predicate != null )
                try {
                    query = query.Where( Predicate );
                } catch ( Exception ex ) {
                    throw new SpecificationException( "Error on apply filter specification!", ex );
                }

            return query;
        }

        public ISpec<T> InitEmpty( ) => new NullSpec<T>( );

        public bool IsSatisfiedBy( T entity ) {
            if ( entity == null )
                throw new ArgumentNullException( nameof( entity ) );

            if ( Predicate == null )
                throw new InvalidSpecificationException( "Predicate cannot be null" );

            var predicate = Predicate.Compile( );
            return predicate( entity );
        }

        public ISpec<T> Not( ) => new NotSpec<T>( this );

        public ISpec<T> Or( ISpec<T> other ) => new OrSpec<T>( this, other );

        public ISpec<T> Or( Expression<Func<T, bool>> other ) {
            if ( other == null )
                throw new ArgumentNullException( nameof( other ) );

            return new OrSpec<T>( this, new ExpressionSpec<T>( other ) );
        }

        public ISpec<T> Order<TProperty>( Expression<Func<T, TProperty>> property ) =>
            new OrderSpec<T, TProperty>( this, property );

        public ISpec<T> OrderDescending<TProperty>( Expression<Func<T, TProperty>> property ) =>
            new OrderDescendingSpec<T, TProperty>( this, property );

        public ISpec<T> DistinctBy<TProperty>( Expression<Func<T, TProperty>> property ) =>
           new DistinctSpec<T, TProperty>( this, property );

        public IQueryable<T> Prepare( IQueryable<T> query ) {
            if ( query == null )
                throw new ArgumentNullException( nameof( query ) );

            return Processed( Sorted( Filtered( query ) ) );
        }

        public IQueryable<T> Processed( IQueryable<T> query ) {
            if ( PostProcess != null )
                try {
                    query = PostProcess( query );
                } catch ( Exception ex ) {
                    throw new SpecificationException( "Error on apply post process specification!", ex );
                }

            return query;
        }

        public IQueryable<T> Sorted( IQueryable<T> query ) {
            if ( Sort != null )
                try {
                    query = Sort( query );
                } catch ( Exception ex ) {
                    throw new SpecificationException( "Error on apply sort specification!", ex );
                }

            return query;
        }

        public ISpec<T> Skip( int amount ) => new SkipSpec<T>( this, amount );

        public ISpec<T> Take( int amount ) => new TakeSpec<T>( this, amount );

        public static implicit operator Expression<Func<T, bool>>( SpecBuilder<T> spec ) => spec.Predicate;
    }
}