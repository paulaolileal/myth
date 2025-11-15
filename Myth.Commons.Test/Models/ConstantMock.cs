using System.Runtime.CompilerServices;
using Myth.ValueObjects;

namespace Myth.Commons.Test.Models;

internal class ConstantMock : Constant<ConstantMock, int> {
	public static readonly ConstantMock One = CreateWithCallerName( 1 );  // Usa CallerMemberName
	public static readonly ConstantMock Two = CreateWithCallerName( 2 );  // Usa CallerMemberName

	public ConstantMock( string name, int value ) : base( name, value ) { }
}
