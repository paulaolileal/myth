using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;

namespace Myth.Extensions {

    public static class UrlExtension {

        private static object GetValueEncoded( this PropertyInfo property, object obj ) {
            var value = property.GetValue( obj );
            if ( property.PropertyType == typeof( string ) )
                value = HttpUtility.UrlEncode( value as string );
            else if ( property.PropertyType == typeof( bool ) )
                value = Convert.ToBoolean( value );

            return value;
        }

        private static object GetValueEncoded( this object value ) {
            var property = value.GetType( );
            if ( property == typeof( string ) )
                value = HttpUtility.UrlEncode( value as string );
            else if ( property == typeof( bool ) )
                value = Convert.ToBoolean( value );

            return value;
        }

        public static string ToQuery<T>( this T value ) {
            var properties = new Stack<PropertyInfo>( value.GetType( ).GetProperties( ) );
            var prop = properties.Pop( );
            var query = $"?{ prop.Name }={ prop.GetValueEncoded( value ) }";

            while ( properties.TryPop( out var property ) ) {
                if ( property.GetValue( value ) != null )
                    query += $"&{property.Name}={property.GetValueEncoded( value )}";
            }

            return query;
        }

        public static string IncludeQuery( this string route, params (string, object)[ ] values ) {
            var properties = new Stack<(string, object)>( values );
            var prop = properties.Pop( );

            if ( !route.Contains( "?" ) )
                route += $"?{ prop.Item1 }={ prop.Item2.GetValueEncoded( ) }";

            while ( properties.TryPop( out var property ) ) {
                if ( property.Item2 != null )
                    route += $"&{property.Item1}={property.Item2.GetValueEncoded( )}";
            }

            return route;
        }
    }
}