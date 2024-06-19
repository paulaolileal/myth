using System.Web;

namespace Myth.Extensions {

	public static class UrlExtension {

		public static object? EncodeAsUrl( this object value ) {
			var property = value.GetType( );

			if ( property == typeof( string ) )
				value = HttpUtility.UrlEncode( value as string )!;
			else if ( property == typeof( bool ) )
				value = Convert.ToBoolean( value );

			return value;
		}
	}
}