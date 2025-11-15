using System.Net;
using FluentAssertions;
using Myth.Models;

namespace Myth.Guard.Test;

/// <summary>
/// Tests for ValidationError model
/// </summary>
public class ValidationErrorTests {

	[Fact]
	public void Constructor_WithAllParameters_ShouldCreateError( ) {
		// Arrange
		const string field = "TestField";
		const string message = "Test message";
		const string code = "TEST_CODE";
		const HttpStatusCode statusCode = HttpStatusCode.BadRequest;

		// Act
		var error = new ValidationError( field, message, code, statusCode );

		// Assert
		error.Should( ).NotBeNull( );
		error.Field.Should( ).Be( field );
		error.Message.Should( ).Be( message );
		error.Code.Should( ).Be( code );
		error.StatusCode.Should( ).Be( statusCode );
	}

	[Fact]
	public void Constructor_WithDefaults_ShouldUseDefaultValues( ) {
		// Arrange
		// Act
		var error = new ValidationError( );

		// Assert
		error.Should( ).NotBeNull( );
		error.Field.Should( ).Be( string.Empty );
		error.Message.Should( ).Be( string.Empty );
		error.Code.Should( ).Be( "VIOLATION" );
		error.StatusCode.Should( ).Be( HttpStatusCode.BadRequest );
	}

	[Fact]
	public void Constructor_WithCustomCode_ShouldSetCode( ) {
		// Arrange
		const string field = "Email";
		const string message = "Invalid email format";
		const string code = "INVALID_EMAIL";

		// Act
		var error = new ValidationError( field, message, code, HttpStatusCode.BadRequest );

		// Assert
		error.Code.Should( ).Be( code );
		error.StatusCode.Should( ).Be( HttpStatusCode.BadRequest );
	}

	[Theory]
	[InlineData( "" )]
	[InlineData( null )]
	public void Constructor_WithNullOrEmptyField_ShouldAcceptValue( string? field ) {
		// Act
		var error = new ValidationError( field, "Message", "CODE", HttpStatusCode.BadRequest );

		// Assert
		error.Field.Should( ).Be( field );
	}

	[Theory]
	[InlineData( "" )]
	[InlineData( null )]
	public void Constructor_WithNullOrEmptyMessage_ShouldAcceptValue( string? message ) {
		// Act
		var error = new ValidationError( "Field", message, "CODE", HttpStatusCode.BadRequest );

		// Assert
		error.Message.Should( ).Be( message );
	}

	[Theory]
	[InlineData( "" )]
	[InlineData( null )]
	public void Constructor_WithNullOrEmptyCode_ShouldAcceptValue( string? code ) {
		// Act
		var error = new ValidationError( "Field", "Message", code, HttpStatusCode.BadRequest );

		// Assert
		error.Code.Should( ).Be( code );
	}

	[Theory]
	[InlineData( HttpStatusCode.OK )]
	[InlineData( HttpStatusCode.BadRequest )]
	[InlineData( HttpStatusCode.Unauthorized )]
	[InlineData( HttpStatusCode.Forbidden )]
	[InlineData( HttpStatusCode.UnprocessableEntity )]
	[InlineData( HttpStatusCode.InternalServerError )]
	public void Constructor_WithVariousStatusCodes_ShouldSetStatusCode( HttpStatusCode statusCode ) {
		// Act
		var error = new ValidationError( "Field", "Message", "CODE", statusCode );

		// Assert
		error.StatusCode.Should( ).Be( statusCode );
	}

	[Fact]
	public void ValidationError_ShouldHaveCorrectProperties( ) {
		// Arrange
		var error1 = new ValidationError( "Field", "Message", "CODE", HttpStatusCode.BadRequest );
		var error2 = new ValidationError( "Field", "Different Message", "CODE", HttpStatusCode.BadRequest );

		// Assert
		error1.Field.Should( ).Be( "Field" );
		error1.Message.Should( ).Be( "Message" );
		error2.Message.Should( ).Be( "Different Message" );
	}

	[Fact]
	public void ToString_ShouldReturnMeaningfulString( ) {
		// Arrange
		var error = new ValidationError( "Email", "Invalid format", "INVALID_EMAIL", HttpStatusCode.BadRequest );

		// Act
		var toString = error.ToString( );

		// Assert
		toString.Should( ).NotBeNullOrWhiteSpace( );
		toString.Should( ).Contain( "Email" );
		toString.Should( ).Contain( "Invalid format" );
		toString.Should( ).Contain( "INVALID_EMAIL" );
	}

	[Fact]
	public void Properties_ShouldBeAccessible( ) {
		// Arrange
		var error = new ValidationError( "TestField", "Test message", "TEST_CODE", HttpStatusCode.UnprocessableEntity );

		// Act & Assert
		error.Field.Should( ).Be( "TestField" );
		error.Message.Should( ).Be( "Test message" );
		error.Code.Should( ).Be( "TEST_CODE" );
		error.StatusCode.Should( ).Be( HttpStatusCode.UnprocessableEntity );
	}
}
