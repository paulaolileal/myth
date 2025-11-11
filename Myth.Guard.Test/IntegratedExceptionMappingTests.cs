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
/// Tests for combining automatic and manual exception mapping
/// </summary>
public class IntegratedExceptionMappingTests {

	private readonly RequestDelegate _next;
	private readonly ILogger<GuardExceptionMiddleware> _logger;
	private readonly IWebHostEnvironment _environment;

	public IntegratedExceptionMappingTests( ) {
		_next = Substitute.For<RequestDelegate>( );
		_logger = Substitute.For<ILogger<GuardExceptionMiddleware>>( );
		_environment = Substitute.For<IWebHostEnvironment>( );
		_environment.EnvironmentName.Returns( Environments.Development );
	}

	[Fact]
	public async Task AutoMapPlusManualMapping_ShouldBothWork( ) {
		// Arrange
		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var options = new GuardOptions { Environment = _environment };

		// First apply auto-mapping
		options.AutoMapCommonExceptions( );

		// Then add manual mapping for custom exception
		options.MapException<FileNotFoundException>( )
			.WithStatusCode( 404 )
			.WithErrorCode( "FILE_NOT_FOUND" )
			.WithResponse( ex => new {
				error = "The requested file was not found",
				fileName = ex.FileName,
				customField = "manual_mapping"
			} )
			.Build( );

		var middleware = new GuardExceptionMiddleware( _next, options, _logger );

		// Test auto-mapped exception
		var argumentException = new ArgumentNullException( "testParam", "Test message" );
		_next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw argumentException );

		// Act - Test auto-mapped exception
		await middleware.InvokeAsync( context );

		// Assert - Auto-mapped exception should work
		context.Response.StatusCode.Should( ).Be( 400 );
		context.Response.ContentType.Should( ).Contain( "application/json" );

		// Read response body
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );

		var responseObj = JsonSerializer.Deserialize<JsonElement>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj.GetProperty( "error" ).GetString( ).Should( ).Be( "Test message (Parameter 'testParam')" );
		responseObj.GetProperty( "parameter" ).GetString( ).Should( ).Be( "testParam" );
	}

	[Fact]
	public async Task ManualMapping_ShouldOverrideAutoMapping( ) {
		// Arrange
		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var options = new GuardOptions { Environment = _environment };

		// First apply auto-mapping
		options.AutoMapCommonExceptions( );

		// Then override ArgumentNullException with custom mapping
		options.MapException<ArgumentNullException>( )
			.WithStatusCode( 422 ) // Different from auto-mapping (400)
			.WithErrorCode( "CUSTOM_ARGUMENT_NULL" )
			.WithResponse( ex => new {
				error = "Custom argument null handling",
				parameter = ex.ParamName,
				customField = "overridden"
			} )
			.Build( );

		var middleware = new GuardExceptionMiddleware( _next, options, _logger );

		var exception = new ArgumentNullException( "testParam", "Test message" );
		_next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw exception );

		// Act
		await middleware.InvokeAsync( context );

		// Assert - Should use manual mapping instead of auto-mapping
		context.Response.StatusCode.Should( ).Be( 422 ); // Custom status, not 400
		context.Response.ContentType.Should( ).Contain( "application/json" );

		// Read response body
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );

		var responseObj = JsonSerializer.Deserialize<JsonElement>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj.GetProperty( "error" ).GetString( ).Should( ).Be( "Custom argument null handling" );
		responseObj.GetProperty( "parameter" ).GetString( ).Should( ).Be( "testParam" );
		responseObj.GetProperty( "customField" ).GetString( ).Should( ).Be( "overridden" );
	}

	[Fact]
	public async Task DefaultHandler_ShouldHandleUnmappedExceptions( ) {
		// Arrange
		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var options = new GuardOptions { Environment = _environment };

		// Apply auto-mapping (includes default handler)
		options.AutoMapCommonExceptions( );

		var middleware = new GuardExceptionMiddleware( _next, options, _logger );

		// Throw an exception that's not specifically mapped
		var exception = new FormatException( "Invalid format detected" );
		_next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw exception );

		// Act
		await middleware.InvokeAsync( context );

		// Assert - Should use default handler (500 with dev details)
		context.Response.StatusCode.Should( ).Be( 500 );
		context.Response.ContentType.Should( ).Contain( "application/json" );

		// Read response body
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );

		var responseObj = JsonSerializer.Deserialize<JsonElement>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj.GetProperty( "error" ).GetString( ).Should( ).Be( "Invalid format detected" );
		responseObj.TryGetProperty( "trace", out var trace ).Should( ).BeTrue( );
		trace.ValueKind.Should( ).Be( JsonValueKind.String );
	}

	[Fact]
	public async Task CustomDefaultHandler_ShouldOverrideAutoDefault( ) {
		// Arrange
		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var options = new GuardOptions { Environment = _environment };

		// Apply auto-mapping first
		options.AutoMapCommonExceptions( );

		// Then override the default handler
		options.MapDefaultException( )
			.WithStatusCode( 503 ) // Service Unavailable instead of 500
			.WithErrorCode( "CUSTOM_ERROR" )
			.WithResponse( ex => new {
				error = "Custom error handling",
				type = ex.GetType( ).Name,
				customField = "custom_default"
			} )
			.Build( );

		var middleware = new GuardExceptionMiddleware( _next, options, _logger );

		// Throw an exception that's not specifically mapped
		var exception = new FormatException( "Invalid format detected" );
		_next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw exception );

		// Act
		await middleware.InvokeAsync( context );

		// Assert - Should use custom default handler
		context.Response.StatusCode.Should( ).Be( 503 );
		context.Response.ContentType.Should( ).Contain( "application/json" );

		// Read response body
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );

		var responseObj = JsonSerializer.Deserialize<JsonElement>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj.GetProperty( "error" ).GetString( ).Should( ).Be( "Custom error handling" );
		responseObj.GetProperty( "type" ).GetString( ).Should( ).Be( "FormatException" );
		responseObj.GetProperty( "customField" ).GetString( ).Should( ).Be( "custom_default" );
	}
}