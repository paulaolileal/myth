using System.Runtime.CompilerServices;
using Myth.ValueObjects;

namespace Myth.Commons.Test.Models;

internal class ConstantMock( string name, int value ) : Constant<ConstantMock, int>( name, value ) {
	public static readonly ConstantMock One = CreateWithCallerName( 1 );  // Usa CallerMemberName
	public static readonly ConstantMock Two = CreateWithCallerName( 2 );  // Usa CallerMemberName
}
