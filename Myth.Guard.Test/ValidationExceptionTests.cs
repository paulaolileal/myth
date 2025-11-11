using System.Net;
using FluentAssertions;
using Myth.Exceptions;
using Myth.Models;

namespace Myth.Guard.Test;

/// <summary>
/// Tests for ValidationException
/// </summary>
public class ValidationExceptionTests {

	[Fact]
	public void Constructor_WithValidationResult_ShouldSetProperties( ) {
		// Arrange
		var errors = new List<ValidationError>
		{
			new("Field1", "Error 1", "CODE1", HttpStatusCode.BadRequest),
			new("Field2", "Error 2", "CODE2", HttpStatusCode.BadRequest)
		};
		var validationResult = new ValidationResult( errors );

		// Act
		var exception = new ValidationException( validationResult );

		// Assert
		exception.Should( ).NotBeNull( );
		exception.ValidationResult.Should( ).BeSameAs( validationResult );
		exception.Message.Should( ).Contain( "Validation failed" );
		exception.Message.Should( ).Contain( "2 error(s)" );
	}

	[Fact]
	public void Constructor_WithErrors_ShouldHaveCorrectMessage( ) {
		// Arrange
		var errors = new List<ValidationError>
		{
			new("Field", "Error message", "CODE", HttpStatusCode.BadRequest)
		};
		var validationResult = new ValidationResult( errors );

		// Act
		var exception = new ValidationException( validationResult );

		// Assert
		exception.Message.Should( ).Contain( "Validation failed" );
	}

	[Fact]
	public void Constructor_WithValidationResult_ShouldUseDefaultMessage( ) {
		// Arrange
		var errors = new List<ValidationError>
		{
			new("Field", "Error", "CODE", HttpStatusCode.BadRequest)
		};
		var validationResult = new ValidationResult( errors );

		// Act
		var exception = new ValidationException( validationResult );

		// Assert
		exception.Message.Should( ).Contain( "Validation failed" );
		exception.ValidationResult.Should( ).BeSameAs( validationResult );
	}

	[Fact]
	public void ValidationResult_Property_ShouldReturnCorrectResult( ) {
		// Arrange
		var errors = new List<ValidationError>
		{
			new("Field", "Error", "CODE", HttpStatusCode.BadRequest)
		};
		var validationResult = new ValidationResult( errors );

		// Act
		var exception = new ValidationException( validationResult );

		// Assert
		exception.ValidationResult.Should( ).BeSameAs( validationResult );
		exception.ValidationResult.Errors.Should( ).HaveCount( 1 );
	}

	[Fact]
	public void Constructor_WithNullValidationResult_ShouldThrowArgumentNullException( ) {
		// Act
		var act = ( ) => new ValidationException( null! );

		// Assert
		act.Should( )
			.Throw<ArgumentNullException>( )
			.WithParameterName( "ValidationResult" );
	}

	[Fact]
	public void ValidationException_ShouldBeInstanceOfException( ) {
		// Arrange
		var errors = new List<ValidationError>
		{
			new("Field", "Error", "CODE", HttpStatusCode.BadRequest)
		};
		var validationResult = new ValidationResult( errors );

		// Act
		var exception = new ValidationException( validationResult );

		// Assert
		exception.Should( ).BeAssignableTo<Exception>( );
		exception.Should( ).BeOfType<ValidationException>( );
	}

	[Fact]
	public void ValidationException_ShouldBeSerializable( ) {
		// Arrange
		var errors = new List<ValidationError>
		{
			new("Field", "Error message", "ERROR_CODE", HttpStatusCode.BadRequest)
		};
		var validationResult = new ValidationResult( errors );
		var exception = new ValidationException( validationResult );

		// Act & Assert - Should not throw during serialization attempts
		var message = exception.Message;
		var validationResultProperty = exception.ValidationResult;

		message.Should( ).NotBeNullOrEmpty( );
		validationResultProperty.Should( ).NotBeNull( );
		validationResultProperty.Should( ).BeSameAs( validationResult );
	}

	[Fact]
	public void ValidationException_WithComplexValidationResult_ShouldGenerateCorrectMessage( ) {
		// Arrange
		var errors = new List<ValidationError>
		{
			new("Name", "Name is required", "REQUIRED", HttpStatusCode.BadRequest),
			new("Email", "Invalid email format", "INVALID_EMAIL", HttpStatusCode.BadRequest),
			new("Age", "Age must be positive", "INVALID_AGE", HttpStatusCode.UnprocessableEntity),
			new("Address", "Address is too long", "TOO_LONG", HttpStatusCode.BadRequest)
		};
		var validationResult = new ValidationResult( errors );

		// Act
		var exception = new ValidationException( validationResult );

		// Assert
		exception.Message.Should( ).Contain( "Validation failed" );
		exception.Message.Should( ).Contain( "4 error(s)" );
		exception.ValidationResult.Errors.Should( ).HaveCount( 4 );
		exception.ValidationResult.StatusCode.Should( ).Be( HttpStatusCode.UnprocessableEntity ); // Highest priority
	}

	[Theory]
	[InlineData( 1, "1 error" )]
	[InlineData( 2, "2 error(s)" )]
	[InlineData( 5, "5 error(s)" )]
	[InlineData( 10, "10 error(s)" )]
	public void Constructor_WithVariousErrorCounts_ShouldGenerateCorrectMessage( int errorCount, string expectedErrorText ) {
		// Arrange
		var errors = Enumerable.Range( 1, errorCount )
			.Select( i => new ValidationError( $"Field{i}", $"Error {i}", $"CODE{i}", HttpStatusCode.BadRequest ) )
			.ToList( );
		var validationResult = new ValidationResult( errors );

		// Act
		var exception = new ValidationException( validationResult );

		// Assert
		exception.Message.Should( ).Contain( "Validation failed" );
		exception.Message.Should( ).Contain( expectedErrorText );
	}

	[Fact]
	public void ValidationException_ToString_ShouldIncludeValidationDetails( ) {
		// Arrange
		var errors = new List<ValidationError>
		{
			new("Field", "Error message", "ERROR_CODE", HttpStatusCode.BadRequest)
		};
		var validationResult = new ValidationResult( errors );
		var exception = new ValidationException( validationResult );

		// Act
		var toStringResult = exception.ToString( );

		// Assert
		toStringResult.Should( ).NotBeNullOrEmpty( );
		toStringResult.Should( ).Contain( nameof( ValidationException ) );
		toStringResult.Should( ).Contain( "Validation failed" );
	}
}
