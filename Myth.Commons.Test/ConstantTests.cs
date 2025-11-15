using FluentAssertions;
using Myth.Commons.Test.Models;

namespace Myth.Commons.Test;

public class ConstantTests {

	[Fact]
	public void Constant_should_have_options_and_implicit_values( ) {
		// Act
		var options = ConstantMock.GetOptions( );
		var test = ConstantMock.One == 1;

		// Assert
		options.Should( ).Be( "(One): 1 | (Two): 2" );
		test.Should( ).BeTrue( );
	}
}
