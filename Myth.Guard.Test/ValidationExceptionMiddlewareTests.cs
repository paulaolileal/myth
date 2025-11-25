using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Myth.Constants;
using Myth.Exceptions;
using Myth.Extensions;
using Myth.Middlewares;
using Myth.Models;
using NSubstitute;

namespace Myth.Guard.Test;

/// <summary>
/// Tests for ValidationExceptionMiddleware
/// </summary>
public class ValidationExceptionMiddlewareTests {
	private readonly RequestDelegate _next;
	private readonly ValidationExceptionMiddleware _middleware;

	public ValidationExceptionMiddlewareTests( ) {
		_next = Substitute.For<RequestDelegate>( );
		_middleware = new ValidationExceptionMiddleware( _next );
	}

	[Fact]
	public async Task InvokeAsync_WithNoException_ShouldCallNext( ) {
		// Arrange
		var context = new DefaultHttpContext( );
		_next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => { /* No exception */ } );

		// Act
		await _middleware.InvokeAsync( context );

		// Assert
		await _next.Received( 1 ).Invoke( context );
	}

	[Fact]
	public async Task InvokeAsync_WithNonValidationException_ShouldRethrow( ) {
		// Arrange
		var context = new DefaultHttpContext( );
		var exception = new InvalidOperationException( "Some other error" );
		_next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw exception );

		// Act
		var act = async ( ) => await _middleware.InvokeAsync( context );

		// Assert
		await act.Should( ).ThrowAsync<InvalidOperationException>( )
			.WithMessage( "Some other error" );
	}

	[Fact]
	public async Task InvokeAsync_WithValidationException_ShouldHandleAndReturnBadRequest( ) {
		// Arrange
		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var validationErrors = new List<ValidationError>
		{
			new("Name", "Name is required", "REQUIRED", HttpStatusCode.BadRequest),
			new("Email", "Invalid email format", "INVALID_EMAIL", HttpStatusCode.BadRequest)
		};
		var validationResult = new ValidationResult( validationErrors );
		var validationException = new ValidationException( validationResult );

		_next
			.When( x => x( Arg.Any<HttpContext>( ) ) )
			.Do( x => throw validationException );

		// Act
		await _middleware.InvokeAsync( context );

		// Assert
		context.Response.StatusCode.Should( ).Be( ( int )HttpStatusCode.BadRequest );
		context.Response.ContentType.Should( ).Contain( "application/json" );

		// Read response body
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );
		responseBody.Should( ).NotBeNullOrEmpty( );

		// Deserialize and verify response
		var responseObj = responseBody.FromJson<ValidationErrorResponse>( x => x.UseCaseStrategy( CaseStrategy.CamelCase ) );

		responseObj.Should( ).NotBeNull( );
		responseObj!.Code.Should( ).Be( "MULTIPLE_ERRORS" );
		responseObj.Errors.Should( ).HaveCount( 2 );
		responseObj.Errors.Should( ).Contain( e => e.Field == "Name" && e.Message == "Name is required" && e.Code == "REQUIRED" );
		responseObj.Errors.Should( ).Contain( e => e.Field == "Email" && e.Message == "Invalid email format" && e.Code == "INVALID_EMAIL" );
	}

	[Fact]
	public async Task InvokeAsync_WithValidationException_UnprocessableEntity_ShouldReturnCorrectStatusCode( ) {
		// Arrange
		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var validationErrors = new List<ValidationError>
		{
			new("Field", "Unprocessable data", "UNPROCESSABLE", HttpStatusCode.UnprocessableEntity)
		};
		var validationResult = new ValidationResult( validationErrors );
		var validationException = new ValidationException( validationResult );

		_next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw validationException );

		// Act
		await _middleware.InvokeAsync( context );

		// Assert
		context.Response.StatusCode.Should( ).Be( ( int )HttpStatusCode.UnprocessableEntity );
	}

	[Fact]
	public async Task InvokeAsync_WithValidationException_MixedStatusCodes_ShouldReturnHighestStatusCode( ) {
		// Arrange
		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var validationErrors = new List<ValidationError>
		{
			new("Field1", "Error 1", "ERROR1", HttpStatusCode.BadRequest),
			new("Field2", "Error 2", "ERROR2", HttpStatusCode.UnprocessableEntity),
			new("Field3", "Error 3", "ERROR3", HttpStatusCode.BadRequest)
		};
		var validationResult = new ValidationResult( validationErrors );
		var validationException = new ValidationException( validationResult );

		_next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw validationException );

		// Act
		await _middleware.InvokeAsync( context );

		// Assert
		context.Response.StatusCode.Should( ).Be( ( int )HttpStatusCode.UnprocessableEntity );
	}

	[Fact]
	public async Task InvokeAsync_WithValidationException_EmptyErrors_ShouldReturnBadRequest( ) {
		// Arrange
		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var validationResult = new ValidationResult( [ ] );

		_ = new ValidationException( validationResult );

		// Act
		await _middleware.InvokeAsync( context );

		// Assert
		context.Response.StatusCode.Should( ).Be( ( int )HttpStatusCode.OK ); // Empty errors = valid result

		// Read response body
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );

		responseBody.Should( ).BeNullOrEmpty( );
	}

	[Fact]
	public async Task InvokeAsync_WithValidationException_ShouldHandleWithoutLogging( ) {
		// Arrange
		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var validationResult = new ValidationResult( new List<ValidationError>
		{
			new("Field", "Error", "CODE", HttpStatusCode.BadRequest)
		} );
		var validationException = new ValidationException( validationResult );

		_next
			.When( x => x( Arg.Any<HttpContext>( ) ) )
			.Do( x => throw validationException );

		// Act
		await _middleware.InvokeAsync( context );

		// Assert
		context.Response.StatusCode.Should( ).Be( ( int )HttpStatusCode.BadRequest );
		context.Response.ContentType.Should( ).Contain( "application/json" );
	}

	[Fact]
	public async Task InvokeAsync_WithCustomErrorCodes_ShouldPreserveErrorCodes( ) {
		// Arrange
		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var validationErrors = new List<ValidationError>
		{
			new("Email", "Email already exists", "EMAIL_EXISTS", HttpStatusCode.BadRequest),
			new("Username", "Username taken", "USERNAME_TAKEN", HttpStatusCode.BadRequest),
			new("Age", "Invalid age", "INVALID_AGE", HttpStatusCode.UnprocessableEntity)
		};
		var validationResult = new ValidationResult( validationErrors );
		var validationException = new ValidationException( validationResult );

		_next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw validationException );

		// Act
		await _middleware.InvokeAsync( context );

		// Assert
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );

		var responseObj = JsonSerializer.Deserialize<ValidationErrorResponse>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj!.Errors.Should( ).Contain( e => e.Code == "EMAIL_EXISTS" );
		responseObj.Errors.Should( ).Contain( e => e.Code == "USERNAME_TAKEN" );
		responseObj.Errors.Should( ).Contain( e => e.Code == "INVALID_AGE" );
	}

	[Fact]
	public async Task InvokeAsync_WithSpecialCharactersInMessages_ShouldHandleCorrectly( ) {
		// Arrange
		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var validationErrors = new List<ValidationError>
		{
			new("Field", "Message with \"quotes\" and 'apostrophes'", "CODE", HttpStatusCode.BadRequest),
			new("Field2", "Message with <html> tags", "CODE2", HttpStatusCode.BadRequest),
			new("Field3", "Message with unicode: ñáéíóú", "CODE3", HttpStatusCode.BadRequest)
		};
		var validationResult = new ValidationResult( validationErrors );
		var validationException = new ValidationException( validationResult );

		_next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw validationException );

		// Act
		await _middleware.InvokeAsync( context );

		// Assert
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body, Encoding.UTF8 ).ReadToEndAsync( );

		// Should not throw when deserializing
		var responseObj = JsonSerializer.Deserialize<ValidationErrorResponse>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj.Should( ).NotBeNull( );
		responseObj!.Errors.Should( ).HaveCount( 3 );
		responseObj.Errors.Should( ).Contain( e => e.Message.Contains( "quotes" ) );
		responseObj.Errors.Should( ).Contain( e => e.Message.Contains( "<html>" ) );
		responseObj.Errors.Should( ).Contain( e => e.Message.Contains( "unicode" ) );
	}

	[Theory]
	[InlineData( HttpStatusCode.UnprocessableEntity )]
	[InlineData( HttpStatusCode.Forbidden )]
	[InlineData( HttpStatusCode.InternalServerError )]
	public async Task InvokeAsync_WithHighPriorityStatusCodes_ShouldReturnCorrectStatusCode( HttpStatusCode statusCode ) {
		// Arrange
		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var validationErrors = new List<ValidationError>
		{
			new("Field1", "Error 1", "ERROR1", HttpStatusCode.BadRequest),
			new("Field2", "Error 2", "ERROR2", statusCode), // High priority status
            new("Field3", "Error 3", "ERROR3", HttpStatusCode.Unauthorized)
		};
		var validationResult = new ValidationResult( validationErrors );
		var validationException = new ValidationException( validationResult );

		_next
			.When( x => x( Arg.Any<HttpContext>( ) ) )
			.Do( x => throw validationException );

		// Act
		await _middleware.InvokeAsync( context );

		// Assert
		context.Response.StatusCode.Should( ).Be( ( int )statusCode );
	}

	[Fact]
	public async Task InvokeAsync_WithValidationErrorWithOptions_ShouldIncludeOptionsInResponse( ) {
		// Arrange
		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var optionsList = new List<string> { "1: Active", "2: Inactive", "3: Pending" }.AsReadOnly( );
		var validationErrors = new List<ValidationError>
		{
			new("Status", "Invalid enum value", "INVALID_OPTION", HttpStatusCode.BadRequest, optionsList)
		};
		var validationResult = new ValidationResult( validationErrors );
		var validationException = new ValidationException( validationResult );

		_next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw validationException );

		// Act
		await _middleware.InvokeAsync( context );

		// Assert
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );

		var responseObj = JsonSerializer.Deserialize<ValidationErrorResponse>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj!.Errors.Should( ).HaveCount( 1 );
		var error = responseObj.Errors.First( );
		error.Options.Should( ).NotBeNull( );
		error.Options.Should( ).Contain( "1: Active" );
		error.Options.Should( ).Contain( "2: Inactive" );
		error.Options.Should( ).Contain( "3: Pending" );
	}

	[Fact]
	public async Task InvokeAsync_WithValidationErrorWithoutOptions_ShouldNotIncludeOptionsProperty( ) {
		// Arrange
		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var validationErrors = new List<ValidationError>
		{
			new("Name", "Name is required", "REQUIRED", HttpStatusCode.BadRequest, null) // No options
		};
		var validationResult = new ValidationResult( validationErrors );
		var validationException = new ValidationException( validationResult );

		_next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw validationException );

		// Act
		await _middleware.InvokeAsync( context );

		// Assert
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );

		// Verify JSON does not contain "options" property due to JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)
		responseBody.Should( ).NotContain( "options" );
		responseBody.Should( ).NotContain( "Options" );

		var responseObj = JsonSerializer.Deserialize<ValidationErrorResponse>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj!.Errors.Should( ).HaveCount( 1 );
		var error = responseObj.Errors.First( );
		error.Options.Should( ).BeNull( );
	}
}
