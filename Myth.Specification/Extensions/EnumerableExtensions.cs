using Myth.Interfaces;

namespace Myth.Extensions {

	public static class EnumerableExtensions {

		public static IEnumerable<T> Where<T>( this IEnumerable<T> values, ISpec<T> spec ) => values.Where( spec.Query );

		public static IQueryable<T> Specify<T>( this IEnumerable<T> values, ISpec<T> spec ) => spec.Prepare( values.AsQueryable( ) );

		public static IQueryable<T> Filter<T>( this IQueryable<T> values, ISpec<T> spec ) => values.Where( spec.Predicate );

		public static IQueryable<T> Sort<T>( this IQueryable<T> values, ISpec<T> spec ) => spec.Sort( values );

		public static IQueryable<T> Paginate<T>( this IQueryable<T> values, ISpec<T> spec ) => spec.PostProcess( values );
	}
}