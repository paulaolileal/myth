using FluentAssertions;
using Myth.Models;

namespace Myth.Flow.Actions.Test {

	public class CommandResultTests {

		[Fact]
		public void Success_ShouldCreateSuccessResult( ) {
			// Arrange & Act
			var result = CommandResult.Success( );

			// Assert
			result.IsSuccess.Should( ).BeTrue( );
			result.IsFailure.Should( ).BeFalse( );
			result.ErrorMessage.Should( ).BeNull( );
			result.Exception.Should( ).BeNull( );
		}

		[Fact]
		public void Success_WithMetadata_ShouldIncludeMetadata( ) {
			// Arrange
			var metadata = new Dictionary<string, object> { [ "key" ] = "value" };

			// Act
			var result = CommandResult.Success( metadata );

			// Assert
			result.IsSuccess.Should( ).BeTrue( );
			result.Metadata.Should( ).ContainKey( "key" );
			result.Metadata![ "key" ].Should( ).Be( "value" );
		}

		[Fact]
		public void Failure_ShouldCreateFailureResult( ) {
			// Arrange & Act
			var result = CommandResult.Failure( "Error occurred" );

			// Assert
			result.IsSuccess.Should( ).BeFalse( );
			result.IsFailure.Should( ).BeTrue( );
			result.ErrorMessage.Should( ).Be( "Error occurred" );
		}

		[Fact]
		public void Failure_WithException_ShouldIncludeException( ) {
			// Arrange
			var exception = new InvalidOperationException( "Test error" );

			// Act
			var result = CommandResult.Failure( "Error occurred", exception );

			// Assert
			result.IsFailure.Should( ).BeTrue( );
			result.Exception.Should( ).Be( exception );
			result.ErrorMessage.Should( ).Be( "Error occurred" );
		}

		[Fact]
		public void Failure_WithMetadata_ShouldIncludeMetadata( ) {
			// Arrange
			var metadata = new Dictionary<string, object> { [ "errorCode" ] = 500 };

			// Act
			var result = CommandResult.Failure( "Error", metadata: metadata );

			// Assert
			result.IsFailure.Should( ).BeTrue( );
			result.Metadata.Should( ).ContainKey( "errorCode" );
		}
	}

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