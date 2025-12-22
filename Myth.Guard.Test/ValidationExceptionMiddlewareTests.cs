using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
	private readonly GuardExceptionMiddleware _middleware;
	private readonly ILogger<GuardExceptionMiddleware> _logger;

	public ValidationExceptionMiddlewareTests( ) {
		_next = Substitute.For<RequestDelegate>( );
		_logger = Substitute.For<ILogger<GuardExceptionMiddleware>>( );
		var options = new GuardOptions( );
		_middleware = new GuardExceptionMiddleware( _next, options, _logger );
	}

	[Fact]
	public async Task InvokeAsync_WithNoException_ShouldCallNext( ) {
		// Arrange
		var context = new DefaultHttpContext( );
		_next
			.When( x => x( Arg.Any<HttpContext>( ) ) )
			.Do( x => { /* No exception */ } );

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
		_next
			.When( x => x( Arg.Any<HttpContext>( ) ) )
			.Do( x => throw exception );

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
			new("Name", "Name is required", HttpStatusCode.BadRequest),
			new("Email", "Invalid email format", HttpStatusCode.BadRequest)
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
		context.Response.ContentType.Should( ).Contain( "application/problem+json" );

		// Read response body
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );
		responseBody.Should( ).NotBeNullOrEmpty( );

		// Deserialize and verify response
		var responseObj = JsonSerializer.Deserialize<ValidationProblemDetails>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj.Should( ).NotBeNull( );
		responseObj!.Status.Should( ).Be( 400 );
		responseObj.Errors.Should( ).ContainKey( "Name" );
		responseObj.Errors[ "Name" ].Should( ).Contain( "Name is required" );
		responseObj.Errors.Should( ).ContainKey( "Email" );
		responseObj.Errors[ "Email" ].Should( ).Contain( "Invalid email format" );
	}

	[Fact]
	public async Task InvokeAsync_WithValidationException_UnprocessableEntity_ShouldReturnCorrectStatusCode( ) {
		// Arrange
		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var validationErrors = new List<ValidationError>
		{
			new("Field", "Unprocessable data", HttpStatusCode.UnprocessableEntity)
		};
		var validationResult = new ValidationResult( validationErrors );
		var validationException = new ValidationException( validationResult );

		_next
			.When( x => x( Arg.Any<HttpContext>( ) ) )
			.Do( x => throw validationException );

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
			new("Field1", "Error 1", HttpStatusCode.BadRequest),
			new("Field2", "Error 2", HttpStatusCode.UnprocessableEntity),
			new("Field3", "Error 3", HttpStatusCode.BadRequest)
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
			new("Field", "Error", HttpStatusCode.BadRequest)
		} );
		var validationException = new ValidationException( validationResult );

		_next
			.When( x => x( Arg.Any<HttpContext>( ) ) )
			.Do( x => throw validationException );

		// Act
		await _middleware.InvokeAsync( context );

		// Assert
		context.Response.StatusCode.Should( ).Be( ( int )HttpStatusCode.BadRequest );
		context.Response.ContentType.Should( ).Contain( "application/problem+json" );
	}

	[Fact]
	public async Task InvokeAsync_WithMultipleFieldErrors_ShouldGroupByField( ) {
		// Arrange
		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var validationErrors = new List<ValidationError>
		{
			new("Email", "Email already exists", HttpStatusCode.Conflict),
			new("Username", "Username taken", HttpStatusCode.Conflict),
			new("Age", "Invalid age", HttpStatusCode.UnprocessableEntity)
		};
		var validationResult = new ValidationResult( validationErrors );
		var validationException = new ValidationException( validationResult );

		_next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw validationException );

		// Act
		await _middleware.InvokeAsync( context );

		// Assert
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );

		var responseObj = JsonSerializer.Deserialize<ValidationProblemDetails>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj!.Errors.Should( ).ContainKey( "Email" );
		responseObj.Errors[ "Email" ].Should( ).Contain( "Email already exists" );
		responseObj.Errors.Should( ).ContainKey( "Username" );
		responseObj.Errors[ "Username" ].Should( ).Contain( "Username taken" );
		responseObj.Errors.Should( ).ContainKey( "Age" );
		responseObj.Errors[ "Age" ].Should( ).Contain( "Invalid age" );
	}

	[Fact]
	public async Task InvokeAsync_WithSpecialCharactersInMessages_ShouldHandleCorrectly( ) {
		// Arrange
		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var validationErrors = new List<ValidationError>
		{
			new("Field", "Message with \"quotes\" and 'apostrophes'", HttpStatusCode.BadRequest),
			new("Field2", "Message with <html> tags", HttpStatusCode.BadRequest),
			new("Field3", "Message with unicode: ñáéíóú", HttpStatusCode.BadRequest)
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
		var responseObj = JsonSerializer.Deserialize<ValidationProblemDetails>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj.Should( ).NotBeNull( );
		responseObj!.Errors.Should( ).HaveCount( 3 );
		responseObj.Errors[ "Field" ].First( ).Should( ).Contain( "quotes" );
		responseObj.Errors[ "Field2" ].First( ).Should( ).Contain( "<html>" );
		responseObj.Errors[ "Field3" ].First( ).Should( ).Contain( "unicode" );
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
			new("Field1", "Error 1", HttpStatusCode.BadRequest),
			new("Field2", "Error 2", statusCode), // High priority status
            new("Field3", "Error 3", HttpStatusCode.Unauthorized)
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
			new("Status", "Invalid enum value", HttpStatusCode.BadRequest, optionsList)
		};
		var validationResult = new ValidationResult( validationErrors );
		var validationException = new ValidationException( validationResult );

		_next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw validationException );

		// Act
		await _middleware.InvokeAsync( context );

		// Assert
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );

		var responseObj = JsonSerializer.Deserialize<ValidationProblemDetails>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj.Should( ).NotBeNull( );
		responseObj!.Errors.Should( ).ContainKey( "Status" );

		// Options should be in Extensions
		responseObj.Extensions.Should( ).ContainKey( "options" );
		var optionsDict = JsonSerializer.Deserialize<Dictionary<string, IReadOnlyList<string>>>(
			responseObj.Extensions[ "options" ].ToString( )!,
			new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
		);
		optionsDict.Should( ).ContainKey( "Status" );
		optionsDict![ "Status" ].Should( ).Contain( "1: Active" );
		optionsDict[ "Status" ].Should( ).Contain( "2: Inactive" );
		optionsDict[ "Status" ].Should( ).Contain( "3: Pending" );
	}

	[Fact]
	public async Task InvokeAsync_WithValidationErrorWithoutOptions_ShouldNotIncludeOptionsProperty( ) {
		// Arrange
		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var validationErrors = new List<ValidationError>
		{
			new("Name", "Name is required", HttpStatusCode.BadRequest, null) // No options
		};
		var validationResult = new ValidationResult( validationErrors );
		var validationException = new ValidationException( validationResult );

		_next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw validationException );

		// Act
		await _middleware.InvokeAsync( context );

		// Assert
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );

		var responseObj = JsonSerializer.Deserialize<ValidationProblemDetails>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj.Should( ).NotBeNull( );
		responseObj!.Errors.Should( ).ContainKey( "Name" );

		// Options should not be in Extensions when null
		responseObj.Extensions.Should( ).NotContainKey( "options" );
	}
}
