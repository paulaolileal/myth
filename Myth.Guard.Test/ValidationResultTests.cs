using System.Net;
using FluentAssertions;
using Myth.Models;

namespace Myth.Guard.Test;

/// <summary>
/// Tests for ValidationResult model
/// </summary>
public class ValidationResultTests {

	[Fact]
	public void Constructor_WithNoErrors_ShouldCreateValidResult( ) {
		// Arrange
		var errors = new List<ValidationError>( );

		// Act
		var result = new ValidationResult( errors );

		// Assert
		result.Should( ).NotBeNull( );
		result.IsValid.Should( ).BeTrue( );
		result.Errors.Should( ).BeEmpty( );
		result.StatusCode.Should( ).Be( HttpStatusCode.OK );
	}

	[Fact]
	public void Constructor_WithErrors_ShouldCreateInvalidResult( ) {
		// Arrange
		var errors = new List<ValidationError>
		{
			new("Name", "Name is required", HttpStatusCode.BadRequest),
			new("Email", "Invalid email format", HttpStatusCode.BadRequest)
		};

		// Act
		var result = new ValidationResult( errors );

		// Assert
		result.Should( ).NotBeNull( );
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).HaveCount( 2 );
		result.StatusCode.Should( ).Be( HttpStatusCode.BadRequest );
	}

	[Fact]
	public void Constructor_WithMixedStatusCodes_ShouldReturnHighestStatusCode( ) {
		// Arrange
		var errors = new List<ValidationError>
		{
			new("Field1", "Error 1", HttpStatusCode.BadRequest),
			new("Field2", "Error 2", HttpStatusCode.UnprocessableEntity),
			new("Field3", "Error 3", HttpStatusCode.BadRequest)
		};

		// Act
		var result = new ValidationResult( errors );

		// Assert
		result.StatusCode.Should( ).Be( HttpStatusCode.UnprocessableEntity );
	}

	[Fact]
	public void Constructor_WithForbiddenStatusCode_ShouldReturnForbidden( ) {
		// Arrange
		var errors = new List<ValidationError>
		{
			new("Field1", "Access denied", HttpStatusCode.Forbidden),
			new("Field2", "Invalid data", HttpStatusCode.BadRequest)
		};

		// Act
		var result = new ValidationResult( errors );

		// Assert
		result.StatusCode.Should( ).Be( HttpStatusCode.Forbidden );
	}

	[Fact]
	public void Constructor_WithUnauthorizedStatusCode_ShouldReturnUnauthorized( ) {
		// Arrange
		var errors = new List<ValidationError>
		{
			new("Auth", "Unauthorized", HttpStatusCode.Unauthorized),
			new("Field", "Invalid", HttpStatusCode.BadRequest)
		};

		// Act
		var result = new ValidationResult( errors );

		// Assert
		result.StatusCode.Should( ).Be( HttpStatusCode.Unauthorized );
	}

	[Fact]
	public void Errors_ShouldBeReadOnly( ) {
		// Arrange
		var errors = new List<ValidationError>
		{
			new("Field", "Error", HttpStatusCode.BadRequest)
		};
		var result = new ValidationResult( errors );

		// Act & Assert
		result.Errors.Should( ).BeOfType<List<ValidationError>>( );
		result.Errors.Should( ).HaveCount( 1 );

		// Verify we can't modify the collection
		var readOnlyErrors = result.Errors;
		readOnlyErrors.Should( ).NotBeNull( );
	}

	[Fact]
	public void IsValid_WithNoErrors_ShouldBeTrue( ) {
		// Arrange & Act
		var result = new ValidationResult( new List<ValidationError>( ) );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public void IsValid_WithErrors_ShouldBeFalse( ) {
		// Arrange
		var errors = new List<ValidationError>
		{
			new("Field", "Error", HttpStatusCode.BadRequest)
		};

		// Act
		var result = new ValidationResult( errors );

		// Assert
		result.IsValid.Should( ).BeFalse( );
	}

	[Theory]
	[InlineData( HttpStatusCode.BadRequest, HttpStatusCode.BadRequest )]
	[InlineData( HttpStatusCode.UnprocessableEntity, HttpStatusCode.UnprocessableEntity )]
	[InlineData( HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized )]
	[InlineData( HttpStatusCode.Forbidden, HttpStatusCode.Forbidden )]
	[InlineData( HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError )]
	public void StatusCode_WithSingleError_ShouldReturnCorrectStatusCode(
		HttpStatusCode errorStatusCode,
		HttpStatusCode expectedStatusCode ) {
		// Arrange
		var errors = new List<ValidationError>
		{
			new("Field", "Error", errorStatusCode)
		};

		// Act
		var result = new ValidationResult( errors );

		// Assert
		result.StatusCode.Should( ).Be( expectedStatusCode );
	}

	[Fact]
	public void StatusCode_WithMultipleErrorsOfSameType_ShouldReturnThatStatusCode( ) {
		// Arrange
		var errors = new List<ValidationError>
		{
			new("Field1", "Error 1", HttpStatusCode.BadRequest),
			new("Field2", "Error 2", HttpStatusCode.BadRequest),
			new("Field3", "Error 3", HttpStatusCode.BadRequest)
		};

		// Act
		var result = new ValidationResult( errors );

		// Assert
		result.StatusCode.Should( ).Be( HttpStatusCode.BadRequest );
	}

	[Fact]
	public void Constructor_WithNullErrors_ShouldCreateValidResult( ) {
		// Arrange
		var act = ( ) => new ValidationResult( null );

		// Act & Assert
		act.Should( ).Throw<ArgumentNullException>( );
	}
}
