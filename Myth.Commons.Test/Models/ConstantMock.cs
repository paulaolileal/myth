using Myth.ValueObjects;

namespace Myth.Commons.Test.Models;

internal class ConstantMock( string name, int value )
	: Constant<ConstantMock, int>( name, value ) {
	public static readonly ConstantMock One = new( nameof( One ), 1 );
	public static readonly ConstantMock Two = new( nameof( Two ), 2 );
}
