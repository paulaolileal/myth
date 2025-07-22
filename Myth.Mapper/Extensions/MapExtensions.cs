using Myth.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myth.Extensions {
	public static class MapExtensions {
		public static TDestination MapTo<TDestination>( this object source, IServiceProvider? sp = null ) {
			var srcType = source.GetType( );
			var destType = typeof( TDestination );

			var registry = ( MapRegistry )( sp ?? DefaultProvider.ServiceProvider! ).GetService( typeof( MapRegistry ) )!;
			var method = typeof( MapRegistry ).GetMethod( nameof( MapRegistry.Map ) )!.MakeGenericMethod( srcType, destType );
			return ( TDestination )method.Invoke( registry, new[ ] { source } )!;
		}

		public static IEnumerable<TDestination> MapTo<TDestination>( this IEnumerable<object> sourceList, IServiceProvider? sp = null ) {
			return sourceList.Select( s => s.MapTo<TDestination>( sp ) ).ToList( );
		}
	}

}
