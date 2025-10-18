using FluentAssertions;
using Myth.Models;

namespace Myth.Flow.Actions.Test {
	public class CommandResultGenericTests {

		[Fact]
		public void Success_ShouldCreateSuccessResultWithData( ) {
			// Arrange & Act
			var result = CommandResult<int>.Success( 42 );

			// Assert
			result.IsSuccess.Should( ).BeTrue( );
			result.IsFailure.Should( ).BeFalse( );
			result.Data.Should( ).Be( 42 );
			result.ErrorMessage.Should( ).BeNull( );
		}

		[Fact]
		public void Success_WithMetadata_ShouldIncludeDataAndMetadata( ) {
			// Arrange
			var metadata = new Dictionary<string, object> { [ "source" ] = "test" };

			// Act
			var result = CommandResult<string>.Success( "data", metadata );

			// Assert
			result.IsSuccess.Should( ).BeTrue( );
			result.Data.Should( ).Be( "data" );
			result.Metadata.Should( ).ContainKey( "source" );
		}

		[Fact]
		public void Failure_ShouldCreateFailureResult( ) {
			// Arrange & Act
			var result = CommandResult<int>.Failure( "Error" );

			// Assert
			result.IsFailure.Should( ).BeTrue( );
			result.IsSuccess.Should( ).BeFalse( );
			result.Data.Should( ).Be( 0 );
			result.ErrorMessage.Should( ).Be( "Error" );
		}

		[Fact]
		public void Failure_WithException_ShouldIncludeException( ) {
			// Arrange
			var exception = new ArgumentException( "Invalid argument" );

			// Act
			var result = CommandResult<string>.Failure( "Failed", exception );

			// Assert
			result.IsFailure.Should( ).BeTrue( );
			result.Exception.Should( ).Be( exception );
			result.Data.Should( ).BeNull( );
		}

		[Fact]
		public void Success_WithComplexType_ShouldStoreCorrectly( ) {
			// Arrange
			var data = new { Id = 1, Name = "Test" };

			// Act
			var result = CommandResult<object>.Success( data );

			// Assert
			result.IsSuccess.Should( ).BeTrue( );
			result.Data.Should( ).Be( data );
		}
	}
}