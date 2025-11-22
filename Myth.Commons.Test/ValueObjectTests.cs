using FluentAssertions;
using Myth.Commons.Test.Models;

namespace Myth.Commons.Test;

public class ValueObjectTests {

	[Fact]
	public void Value_object_should_have_options_and_implicit_values( ) {
		// Arrange
		var valueObject1 = new ValueObjectMock( "Test1" );
		var valueObject2 = new ValueObjectMock( "Test2" );

		// Act
		var compare = valueObject1 != valueObject2;

		_ = valueObject1.GetHashCode( );
		var copy = valueObject1.Clone( );

		// Assert
		compare.Should( ).BeTrue( );
		copy.Should( ).NotBeNull( );
	}
}
