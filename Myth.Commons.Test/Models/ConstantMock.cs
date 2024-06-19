using Myth.ValueObjects;

namespace Myth.Commons.Test.Models {

	internal class ConstantMock : Constant<ConstantMock, int> {
		public static readonly ConstantMock One = new( nameof( One ), 1 );
		public static readonly ConstantMock Two = new( nameof( Two ), 2 );

		public ConstantMock( string name, int value ) : base( name, value ) {
		}
	}
}