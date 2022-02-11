using System;
using System.Reflection;
using System.Web;

namespace Myth.Extensions {

    public static class UrlExtension {

        public static object GetValueEncoded( this PropertyInfo property, object obj ) {
            var value = property.GetValue( obj );
            if ( property.PropertyType == typeof( string ) )
                value = HttpUtility.UrlEncode( value as string );
            else if ( property.PropertyType == typeof( bool ) )
                value = Convert.ToBoolean( value );

            return value;
        }

        public static object GetValueEncoded( this object value ) {
            var property = value.GetType( );
            if ( property == typeof( string ) )
                value = HttpUtility.UrlEncode( value as string );
            else if ( property == typeof( bool ) )
                value = Convert.ToBoolean( value );

            return value;
        }
    }
}