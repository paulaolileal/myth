using Ardalis.SmartEnum;
using Myth.Extensions;

namespace Myth.ValueObjects;

/// <summary>
/// A base for creating constants higly typed
/// </summary>
/// <typeparam name="TConstant">Type of constant</typeparam>
/// <typeparam name="TValue">Type of value</typeparam>
/// <param name="name">Name of option</param>
/// <param name="value">Value of option</param>
public abstract class Constant<TConstant, TValue>( string name, TValue value )
	: SmartEnum<TConstant, TValue>( name, value )
		where TConstant : SmartEnum<TConstant, TValue>
		where TValue : IEquatable<TValue>, IComparable<TValue> {

	public static implicit operator TValue( Constant<TConstant, TValue> constant ) => constant.Value;

	/// <summary>
	/// Get a list of all values of this constant
	/// </summary>
	/// <returns></returns>
	public static string GetOptions( ) =>
		List
			.OrderBy( x => x.Value )
			.Select( x => $"{x.Value}: {x.Name}" )
			.ToStringWithSeparator( " | " );
}
