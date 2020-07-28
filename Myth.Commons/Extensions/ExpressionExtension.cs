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

            if ( type == "int" || ( type == "" && value.All( x => char.IsDigit( x ) ) ) ) {
                var @int = Convert.ToInt32( value );
                constant = Expression.Constant( @int );
            } else if ( type == "short" ) {
                var @short = Convert.ToInt16( value );
                constant = Expression.Constant( @short );
            } else if ( type == "long" ) {
                var @long = Convert.ToInt64( value );
                constant = Expression.Constant( @long );
            } else if ( type == "bool" || ( type == "" && value.ToLower( ) == "true" || value.ToLower( ) == "false" ) ) {
                var @bool = Convert.ToBoolean( value );
                constant = Expression.Constant( @bool );
            } else if ( type == "char" ) {
                var @char = Convert.ToChar( value );
                constant = Expression.Constant( @char );
            }

            return constant;
        }

        public static ConstantExpression GenerateConstant( params string[ ] data ) {
            var value = string.Join( " ", data.Skip( 2 ) );
            return GenerateConstant( value );
        }
    }
}