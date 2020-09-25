using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Myth.Extensions {

    public class ExpressionExtension {

        public static MemberExpression GenerateNavigationProperty<T>( string value, ParameterExpression parameter ) {
            MemberExpression selector = null;
            Expression current = parameter;
            Type currentType = typeof( T );

            foreach ( var part in value.Split( '.' ) ) {
                var prop = PropertyExists( part, currentType );
                if ( prop == null )
                    throw new Exception( $"Property or field {{{part}}} not exists on {{{currentType.Name}}}" );

                selector = Expression.Property( current, prop );
                current = selector;
                currentType = prop.PropertyType;
            }

            return selector;
        }

        public static PropertyInfo PropertyExists( string propertyName, Type type ) =>
            type.GetProperty( propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance );

        public static ConstantExpression GenerateConstant( string data ) {
            var values = data.Split( ":" );
            var type = values.Length > 1 ? values[ 1 ].ToLower( ) : "";
            var value = values.FirstOrDefault( );

            var constant = Expression.Constant( value );
            object castvalue = value;

            if ( type == "int" || ( type == "" && value.All( x => char.IsDigit( x ) ) ) )
                castvalue = Convert.ToInt32( value );
            else if ( type == "short" )
                castvalue = Convert.ToInt16( value );
            else if ( type == "long" )
                castvalue = Convert.ToInt64( value );
            else if ( type == "bool" || ( type == "" && value.ToLower( ) == "true" || value.ToLower( ) == "false" ) )
                castvalue = Convert.ToBoolean( value );
            else if ( type == "char" )
                castvalue = Convert.ToChar( value );
            else if ( value.ToLower( ) == "null" )
                castvalue = null;

            constant = Expression.Constant( castvalue );

            return constant;
        }

        public static ConstantExpression GenerateConstant( params string[ ] data ) {
            var value = string.Join( " ", data.Skip( 2 ) );
            return GenerateConstant( value );
        }
    }
}