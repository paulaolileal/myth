using Myth.Mapper;

namespace Myth.Extensions {

	public static class MapExtensions {

		public static TDestination MapTo<TDestination>( this object source, IServiceProvider? sp = null ) {
			var srcType = source.GetType( );
			var destType = typeof( TDestination );

			var serviceProvider = sp ?? DefaultProvider.ServiceProvider;

			var registry = ( MapRegistry )serviceProvider!.GetService( typeof( MapRegistry ) )!;

			var method = typeof( MapRegistry )
				.GetMethod( nameof( MapRegistry.Map ) )!
				.MakeGenericMethod( srcType, destType );

			return ( TDestination )method.Invoke( registry, [ source ] )!;
		}

		public static IEnumerable<TDestination> MapTo<TDestination>( this IEnumerable<object> sourceList, IServiceProvider? sp = null ) {
			return sourceList
				.Select( s => s.MapTo<TDestination>( sp ) )
				.ToList( );
		}
	}
}