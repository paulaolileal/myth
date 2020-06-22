using System.Collections.Generic;

namespace Myth.ValueObjects {

    public class Constant<T1, T2>: Constant<T1> {
        public T2 Value2 { get; private set; }

        public Constant( T1 value, T2 value2 ) : base( value ) {
            Value2 = value2;
        }

        public override string ToString( ) =>
             Value.ToString( ) + " " + Value2.ToString( );

        public virtual bool Equals( Constant<T1, T2> other ) =>
             other.Value.ToString( ) == Value.ToString( ) &&
             other.Value2.ToString( ) == Value2.ToString( );

        public override int GetHashCode( ) =>
             -1937169414 + EqualityComparer<T1>.Default.GetHashCode( Value );

        public override bool Equals( object obj ) =>
             base.Equals( obj );

        public static implicit operator T1( Constant<T1, T2> a ) =>
             a.Value;

        public static implicit operator T2( Constant<T1, T2> a ) =>
             a.Value2;

        public static bool operator !=( Constant<T1, T2> a, Constant<T1, T2> b ) =>
             a.ToString( ) != b.ToString( );

        public static bool operator ==( Constant<T1, T2> a, Constant<T1, T2> b ) =>
             a.ToString( ) == b.ToString( );
    }
}