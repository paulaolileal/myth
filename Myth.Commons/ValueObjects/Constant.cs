using Ardalis.SmartEnum;
using Myth.Extensions;

namespace Myth.ValueObjects;

public abstract class Constant<TConstant, TValue>( string name, TValue value )
	: SmartEnum<TConstant, TValue>( name, value )
	where TConstant : SmartEnum<TConstant, TValue>
	where TValue : IEquatable<TValue>, IComparable<TValue> {

	public static implicit operator TValue( Constant<TConstant, TValue> constant ) => constant.Value;

	public static string GetOptions( ) =>
		List
			.Select( x => $"({x.Name}): {x.Value}" )
			.ToStringWithSeparator( " | " );
}