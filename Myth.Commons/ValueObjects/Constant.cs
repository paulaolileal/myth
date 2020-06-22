using System;
using System.Collections.Generic;

namespace Myth.ValueObjects {

    public class Constant<T>: IEquatable<Constant<T>> {
        public T Value { get; private set; }

        protected Constant( T value ) {
            Value = value;
        }

        public override string ToString( ) =>
             Value.ToString( );

        public virtual bool Equals( Constant<T> other ) =>
             other.Value.ToString( ) == Value.ToString( );

        public override int GetHashCode( ) =>
             -1937169414 + EqualityComparer<T>.Default.GetHashCode( Value );

        public override bool Equals( object obj ) =>
             base.Equals( obj );

        public static implicit operator T( Constant<T> a ) =>
             a.Value;

        public static implicit operator Constant<T>( T a ) =>
             new Constant<T>( a );

        public static bool operator !=( Constant<T> a, Constant<T> b ) =>
             a.ToString( ) != b.ToString( );

        public static bool operator ==( Constant<T> a, Constant<T> b ) =>
             a.ToString( ) == b.ToString( );
    }
}