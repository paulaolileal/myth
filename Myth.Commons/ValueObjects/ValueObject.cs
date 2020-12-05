using System.Collections.Generic;
using System.Linq;

namespace Myth.ValueObjects {

    public abstract class ValueObject {

        protected abstract IEnumerable<object> GetAtomicValues( );

        public static bool operator ==( ValueObject left, ValueObject right ) {
            if ( left is null ^ right is null )
                return false;

            return left is null || left.Equals( right );
        }

        public static bool operator !=( ValueObject left, ValueObject right ) => !( left == right );

        public static IEnumerable<TConstant> ToList<TConstant>( ) where TConstant : ValueObject {
            var type = typeof( TConstant );
            var constants = type
                .GetProperties( )
                .Where( prop => prop.PropertyType == type )
                .Select( x => ( TConstant ) x.GetValue( type, null ) )
                .ToList( );

            return constants;
        }

        public override bool Equals( object obj ) {
            if ( obj == null || obj.GetType( ) != GetType( ) )
                return false;

            var other = ( ValueObject ) obj;
            var thisValues = GetAtomicValues( ).GetEnumerator( );
            var otherValues = other.GetAtomicValues( ).GetEnumerator( );
            while ( thisValues.MoveNext( ) && otherValues.MoveNext( ) ) {
                if ( thisValues.Current is null ^ otherValues.Current is null )
                    return false;

                if ( thisValues.Current != null && !thisValues.Current.Equals( otherValues.Current ) )
                    return false;
            }
            return !thisValues.MoveNext( ) && !otherValues.MoveNext( );
        }

        public override int GetHashCode( ) {
            return GetAtomicValues( )
             .Select( x => x != null ? x.GetHashCode( ) : 0 )
             .Aggregate( ( x, y ) => x ^ y );
        }

        public ValueObject GetCopy( ) =>
             this.MemberwiseClone( ) as ValueObject;
    }
}