using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Myth.Middlewares;
using Myth.Models;
using NSubstitute;

namespace Myth.Guard.Test;

/// <summary>
/// Tests for manual exception mapping configuration
/// </summary>
public class ManualExceptionMappingTests {

	private readonly RequestDelegate _next;
	private readonly ILogger<GuardExceptionMiddleware> _logger;
	private readonly IWebHostEnvironment _environment;

	public ManualExceptionMappingTests( ) {
		_next = Substitute.For<RequestDelegate>( );
		_logger = Substitute.For<ILogger<GuardExceptionMiddleware>>( );
		_environment = Substitute.For<IWebHostEnvironment>( );
		_environment.EnvironmentName.Returns( Environments.Development );
	}

	[Fact]
	public async Task ManualMapping_WithSimpleResponse_ShouldWork( ) {
		// Arrange
		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var options = new GuardOptions { Environment = _environment };

		// Manual mapping without calling Build() - this should fail
		options.GuardException<FileNotFoundException>( )
			.WithStatusCode( 404 )
			.WithErrorCode( "FILE_NOT_FOUND" )
			.WithResponse( ex => new {
				error = "The requested file was not found",
				fileName = ex.FileName
			} );
		// NOTE: Not calling Build() here to test if it fails

		var middleware = new GuardExceptionMiddleware( _next, options, _logger );

		var exception = new FileNotFoundException( "Config file not found", "appsettings.json" );
		_next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw exception );

		// Act & Assert
		var act = async ( ) => await middleware.InvokeAsync( context );

		// This should rethrow because the handler wasn't built/registered
		await act.Should( ).ThrowAsync<FileNotFoundException>( )
			.WithMessage( "*Config file not found*" );
	}

	[Fact]
	public async Task ManualMapping_WithBuildCall_ShouldWork( ) {
		// Arrange
		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var options = new GuardOptions { Environment = _environment };

		// Manual mapping WITH calling Build() - this should work
		options.GuardException<FileNotFoundException>( )
			.WithStatusCode( 404 )
			.WithErrorCode( "FILE_NOT_FOUND" )
			.WithResponse( ex => new {
				error = "The requested file was not found",
				fileName = ex.FileName
			} )
			.Build( );

		var middleware = new GuardExceptionMiddleware( _next, options, _logger );

		var exception = new FileNotFoundException( "Config file not found", "appsettings.json" );
		_next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw exception );

		// Act
		await middleware.InvokeAsync( context );

		// Assert
		context.Response.StatusCode.Should( ).Be( 404 );
		context.Response.ContentType.Should( ).Contain( "application/json" );

		// Read response body
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );
		responseBody.Should( ).NotBeNullOrEmpty( );

		// Deserialize and verify response
		var responseObj = JsonSerializer.Deserialize<JsonElement>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj.GetProperty( "error" ).GetString( ).Should( ).Be( "The requested file was not found" );
		responseObj.GetProperty( "fileName" ).GetString( ).Should( ).Be( "appsettings.json" );
	}

	[Fact]
	public async Task ManualMapping_WithAndMethod_ShouldWork( ) {
		// Arrange
		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var options = new GuardOptions { Environment = _environment };

		// Manual mapping using .And() method - this should automatically build
		options.GuardException<DirectoryNotFoundException>( )
			.WithStatusCode( 404 )
			.WithErrorCode( "DIRECTORY_NOT_FOUND" )
			.WithResponse( ex => new {
				error = "The requested directory was not found",
				path = ex.Message
			} )
			.And( ); // This should call Build() automatically

		var middleware = new GuardExceptionMiddleware( _next, options, _logger );

		var exception = new DirectoryNotFoundException( "/config/missing" );
		_next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw exception );

		// Act
		await middleware.InvokeAsync( context );

		// Assert
		context.Response.StatusCode.Should( ).Be( 404 );
		context.Response.ContentType.Should( ).Contain( "application/json" );

		// Read response body
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );
		responseBody.Should( ).NotBeNullOrEmpty( );

		// Deserialize and verify response
		var responseObj = JsonSerializer.Deserialize<JsonElement>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj.GetProperty( "error" ).GetString( ).Should( ).Be( "The requested directory was not found" );
		responseObj.GetProperty( "path" ).GetString( ).Should( ).Be( "/config/missing" );
	}

	[Fact]
	public async Task ManualMapping_WithImplicitOperator_ShouldWork( ) {
		// Arrange
		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		// Manual mapping using implicit conversion - this should automatically build
		var options = new GuardOptions { Environment = _environment };

		GuardOptions configuredOptions = options.GuardException<UnauthorizedAccessException>( )
			.WithStatusCode( HttpStatusCode.Forbidden )
			.WithErrorCode( "ACCESS_DENIED" )
			.WithResponse( ex => new {
				error = "You do not have permission to perform this action",
				action = "file_access"
			} ); // Implicit conversion should call Build()

		var middleware = new GuardExceptionMiddleware( _next, configuredOptions, _logger );

		var exception = new UnauthorizedAccessException( "Access denied to secure file" );
		_next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw exception );

		// Act
		await middleware.InvokeAsync( context );

		// Assert
		context.Response.StatusCode.Should( ).Be( 403 );
		context.Response.ContentType.Should( ).Contain( "application/json" );

		// Read response body
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );
		responseBody.Should( ).NotBeNullOrEmpty( );

		// Deserialize and verify response
		var responseObj = JsonSerializer.Deserialize<JsonElement>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj.GetProperty( "error" ).GetString( ).Should( ).Be( "You do not have permission to perform this action" );
		responseObj.GetProperty( "action" ).GetString( ).Should( ).Be( "file_access" );
	}

	[Fact]
	public async Task ManualMapping_WithDynamicStatusCode_ShouldWork( ) {
		// Arrange
		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var options = new GuardOptions { Environment = _environment };

		// Manual mapping with dynamic status code
		options.GuardException<HttpRequestException>( )
			.WithStatusCode( ex => {
				// Extract status code from message or default to 502
				if ( ex.Message.Contains( "404" ) )
					return 404;
				if ( ex.Message.Contains( "500" ) )
					return 500;
				return 502;
			} )
			.WithErrorCode( "HTTP_REQUEST_FAILED" )
			.WithResponse( ex => new {
				error = "HTTP request failed",
				details = ex.Message
			} )
			.Build( );

		var middleware = new GuardExceptionMiddleware( _next, options, _logger );

		var exception = new HttpRequestException( "Request failed with 404 Not Found" );
		_next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw exception );

		// Act
		await middleware.InvokeAsync( context );

		// Assert
		context.Response.StatusCode.Should( ).Be( 404 ); // Should extract 404 from message
		context.Response.ContentType.Should( ).Contain( "application/json" );

		// Read response body
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );
		responseBody.Should( ).NotBeNullOrEmpty( );

		// Deserialize and verify response
		var responseObj = JsonSerializer.Deserialize<JsonElement>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj.GetProperty( "error" ).GetString( ).Should( ).Be( "HTTP request failed" );
		responseObj.GetProperty( "details" ).GetString( ).Should( ).Be( "Request failed with 404 Not Found" );
	}

	[Fact]
	public async Task ManualMapping_WithOnBeforeResponse_ShouldWork( ) {
		// Arrange
		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var options = new GuardOptions { Environment = _environment };

		var callbackExecuted = false;
		string? capturedMessage = null;

		// Manual mapping with OnBeforeResponse callback
		options.GuardException<OperationCanceledException>( )
			.WithStatusCode( 499 ) // Client Closed Request
			.WithErrorCode( "OPERATION_CANCELLED" )
			.WithResponse( ex => new {
				error = "Operation was cancelled",
				reason = ex.Message
			} )
			.OnBeforeResponse( ( ex, httpContext ) => {
				callbackExecuted = true;
				capturedMessage = ex.Message;
				httpContext.Response.Headers.Append( "X-Custom-Header", "Operation-Cancelled" );
			} )
			.Build( );

		var middleware = new GuardExceptionMiddleware( _next, options, _logger );

		var exception = new OperationCanceledException( "User cancelled the operation" );
		_next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw exception );

		// Act
		await middleware.InvokeAsync( context );

		// Assert
		context.Response.StatusCode.Should( ).Be( 499 );
		context.Response.ContentType.Should( ).Contain( "application/json" );
		context.Response.Headers.Should( ).ContainKey( "X-Custom-Header" );
		context.Response.Headers[ "X-Custom-Header" ].Should( ).Contain( "Operation-Cancelled" );

		callbackExecuted.Should( ).BeTrue( );
		capturedMessage.Should( ).Be( "User cancelled the operation" );

		// Read response body
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );
		responseBody.Should( ).NotBeNullOrEmpty( );

		// Deserialize and verify response
		var responseObj = JsonSerializer.Deserialize<JsonElement>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj.GetProperty( "error" ).GetString( ).Should( ).Be( "Operation was cancelled" );
		responseObj.GetProperty( "reason" ).GetString( ).Should( ).Be( "User cancelled the operation" );
	}
}
