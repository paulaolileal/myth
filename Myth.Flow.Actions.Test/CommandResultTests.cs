using FluentAssertions;
using Myth.Models;

namespace Myth.Flow.Actions.Test;

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
