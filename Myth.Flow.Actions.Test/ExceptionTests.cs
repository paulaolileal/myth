using FluentAssertions;
using Myth.Models;

namespace Myth.Flow.Actions.Test;

public class ExceptionTests {

	[Fact]
	public void HandlerNotFoundException_ShouldContainMessage( ) {
		// Arrange & Act
		var exception = new HandlerNotFoundException( "Handler not found" );

		// Assert
		exception.Message.Should( ).Be( "Handler not found" );
	}

	[Fact]
	public void HandlerNotFoundException_WithInnerException_ShouldPreserveInner( ) {
		// Arrange
		var inner = new InvalidOperationException( "Inner error" );

		// Act
		var exception = new HandlerNotFoundException( "Handler not found", inner );

		// Assert
		exception.InnerException.Should( ).Be( inner );
	}

	[Fact]
	public void MessageBrokerException_ShouldContainMessage( ) {
		// Arrange & Act
		var exception = new MessageBrokerException( "Broker error" );

		// Assert
		exception.Message.Should( ).Be( "Broker error" );
	}

	[Fact]
	public void CacheException_ShouldContainMessage( ) {
		// Arrange & Act
		var exception = new CacheException( "Cache error" );

		// Assert
		exception.Message.Should( ).Be( "Cache error" );
	}

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
	}
}
