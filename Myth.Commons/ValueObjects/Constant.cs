using Ardalis.SmartEnum;
using Myth.Extensions;

namespace Myth.ValueObjects {

    public abstract class Constant<TConstant, TValue> : SmartEnum<TConstant, TValue>
        where TConstant : SmartEnum<TConstant, TValue>
        where TValue : IEquatable<TValue>, IComparable<TValue> {

        public Constant( string name, TValue value ) : base( name, value ) {
        }

        public static implicit operator TValue( Constant<TConstant, TValue> constant ) => constant.Value;

        public static string GetOptions( ) {
            var optionsList = List
                .Select( x => $"({x.Value}): x.Name" )
                .ToList( );

            return optionsList.ToStringWithSeparator( " | " );
        }
    }
}