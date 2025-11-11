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
/// Tests for GuardExceptionMiddleware with auto-mapped common exceptions
/// </summary>
public class GuardExceptionMiddlewareTests {

	private readonly RequestDelegate _next;
	private readonly ILogger<GuardExceptionMiddleware> _logger;
	private readonly IWebHostEnvironment _environment;
	private readonly GuardOptions _options;
	private readonly GuardExceptionMiddleware _middleware;

	public GuardExceptionMiddlewareTests( ) {
		_next = Substitute.For<RequestDelegate>( );
		_logger = Substitute.For<ILogger<GuardExceptionMiddleware>>( );
		_environment = Substitute.For<IWebHostEnvironment>( );
		_environment.EnvironmentName.Returns( Environments.Development );

		_options = new GuardOptions { Environment = _environment };
		_options.AutoMapCommonExceptions( );

		_middleware = new GuardExceptionMiddleware( _next, _options, _logger );
	}

	[Fact]
	public async Task InvokeAsync_WithNoException_ShouldCallNext( ) {
		// Arrange
		var context = new DefaultHttpContext( );

		// Act
		await _middleware.InvokeAsync( context );

		// Assert
		await _next.Received( 1 ).Invoke( context );
	}

	[Fact]
	public async Task InvokeAsync_WithArgumentNullException_ShouldReturnBadRequest( ) {
		// Arrange
		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var exception = new ArgumentNullException( "testParameter", "This is a test exception" );
		_next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw exception );

		// Act
		await _middleware.InvokeAsync( context );

		// Assert
		context.Response.StatusCode.Should( ).Be( 400 );
		context.Response.ContentType.Should( ).Contain( "application/json" );

		// Read response body
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );
		responseBody.Should( ).NotBeNullOrEmpty( );

		// Deserialize and verify response
		var responseObj = JsonSerializer.Deserialize<JsonElement>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj.GetProperty( "error" ).GetString( ).Should( ).Be( "This is a test exception (Parameter 'testParameter')" );
		responseObj.GetProperty( "parameter" ).GetString( ).Should( ).Be( "testParameter" );
	}

	[Fact]
	public async Task InvokeAsync_WithArgumentException_ShouldReturnBadRequest( ) {
		// Arrange
		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var exception = new ArgumentException( "Invalid argument provided", "testArg" );
		_next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw exception );

		// Act
		await _middleware.InvokeAsync( context );

		// Assert
		context.Response.StatusCode.Should( ).Be( 400 );
		context.Response.ContentType.Should( ).Contain( "application/json" );

		// Read response body
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );

		var responseObj = JsonSerializer.Deserialize<JsonElement>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj.GetProperty( "error" ).GetString( ).Should( ).Be( "Invalid argument provided (Parameter 'testArg')" );
		responseObj.GetProperty( "parameter" ).GetString( ).Should( ).Be( "testArg" );
	}

	[Fact]
	public async Task InvokeAsync_WithInvalidOperationException_ShouldReturnConflict( ) {
		// Arrange
		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var exception = new InvalidOperationException( "Custom operation failed" );
		_next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw exception );

		// Act
		await _middleware.InvokeAsync( context );

		// Assert
		context.Response.StatusCode.Should( ).Be( 409 );
		context.Response.ContentType.Should( ).Contain( "application/json" );

		// Read response body
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );

		var responseObj = JsonSerializer.Deserialize<JsonElement>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj.GetProperty( "error" ).GetString( ).Should( ).Be( "Custom operation failed" );
	}

	[Fact]
	public async Task InvokeAsync_WithUnauthorizedAccessException_ShouldReturnForbidden( ) {
		// Arrange
		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var exception = new UnauthorizedAccessException( "You don't have permission" );
		_next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw exception );

		// Act
		await _middleware.InvokeAsync( context );

		// Assert
		context.Response.StatusCode.Should( ).Be( 403 );
		context.Response.ContentType.Should( ).Contain( "application/json" );

		// Read response body
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );

		var responseObj = JsonSerializer.Deserialize<JsonElement>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj.GetProperty( "error" ).GetString( ).Should( ).Be( "Access denied" );
	}

	[Fact]
	public async Task InvokeAsync_WithNotImplementedException_ShouldReturnNotImplemented( ) {
		// Arrange
		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var exception = new NotImplementedException( "This feature is coming soon" );
		_next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw exception );

		// Act
		await _middleware.InvokeAsync( context );

		// Assert
		context.Response.StatusCode.Should( ).Be( 501 );
		context.Response.ContentType.Should( ).Contain( "application/json" );

		// Read response body
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );

		var responseObj = JsonSerializer.Deserialize<JsonElement>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj.GetProperty( "error" ).GetString( ).Should( ).Be( "This feature is not implemented" );
	}

	[Fact]
	public async Task InvokeAsync_WithTimeoutException_ShouldReturnRequestTimeout( ) {
		// Arrange
		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var exception = new TimeoutException( "Operation took too long" );
		_next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw exception );

		// Act
		await _middleware.InvokeAsync( context );

		// Assert
		context.Response.StatusCode.Should( ).Be( 408 );
		context.Response.ContentType.Should( ).Contain( "application/json" );

		// Read response body
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );

		var responseObj = JsonSerializer.Deserialize<JsonElement>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj.GetProperty( "error" ).GetString( ).Should( ).Be( "The operation timed out" );
	}

	[Fact]
	public async Task InvokeAsync_WithUnhandledException_InDevelopment_ShouldReturnInternalServerErrorWithDetails( ) {
		// Arrange
		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var exception = new DivideByZeroException( "Cannot divide by zero" );
		_next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw exception );

		// Act
		await _middleware.InvokeAsync( context );

		// Assert
		context.Response.StatusCode.Should( ).Be( 500 );
		context.Response.ContentType.Should( ).Contain( "application/json" );

		// Read response body
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );

		var responseObj = JsonSerializer.Deserialize<JsonElement>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj.GetProperty( "error" ).GetString( ).Should( ).Be( "Cannot divide by zero" );
		responseObj.TryGetProperty( "trace", out var trace ).Should( ).BeTrue( );
		trace.ValueKind.Should( ).Be( JsonValueKind.String );
	}

	[Fact]
	public async Task InvokeAsync_WithUnhandledException_InProduction_ShouldReturnInternalServerErrorWithoutDetails( ) {
		// Arrange
		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		// Create fresh production environment
		var productionEnvironment = Substitute.For<IWebHostEnvironment>( );
		productionEnvironment.EnvironmentName.Returns( Environments.Production );

		// Create production options
		var productionOptions = new GuardOptions { Environment = productionEnvironment };
		productionOptions.AutoMapCommonExceptions( );

		var productionMiddleware = new GuardExceptionMiddleware( _next, productionOptions, _logger );

		var exception = new DivideByZeroException( "Cannot divide by zero" );
		_next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw exception );

		// Act
		await productionMiddleware.InvokeAsync( context );

		// Assert
		context.Response.StatusCode.Should( ).Be( 500 );
		context.Response.ContentType.Should( ).Contain( "application/json" );

		// Read response body
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );

		var responseObj = JsonSerializer.Deserialize<JsonElement>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj.GetProperty( "error" ).GetString( ).Should( ).Be( "An internal error occurred" );

		if ( responseObj.TryGetProperty( "trace", out var traceProperty ) ) {
			traceProperty.ValueKind.Should( ).Be( JsonValueKind.Null );
		}
	}

	[Fact]
	public async Task InvokeAsync_WithUnmappedException_ShouldRethrow( ) {
		// Arrange
		var context = new DefaultHttpContext( );

		// Create options without auto-mapping
		var emptyOptions = new GuardOptions( );
		var emptyMiddleware = new GuardExceptionMiddleware( _next, emptyOptions, _logger );

		var exception = new ArgumentNullException( "testParameter" );
		_next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw exception );

		// Act
		var act = async ( ) => await emptyMiddleware.InvokeAsync( context );

		// Assert
		await act.Should( ).ThrowAsync<ArgumentNullException>( )
			.WithMessage( "*testParameter*" );
	}
}