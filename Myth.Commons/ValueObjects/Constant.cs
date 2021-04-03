using Ardalis.SmartEnum;

namespace Myth.ValueObjects {

    public abstract class Constant<T> : SmartEnum<Constant<T>> {
        private static int _value;
        public Constant( string name, int value ) : base( name, value ) { }

        public Constant( string name ) : this( name, ++_value ) { }

        public static implicit operator string( Constant<T> constant ) => constant.Name;
        public static implicit operator int( Constant<T> constant ) => constant.Value;
    }
}