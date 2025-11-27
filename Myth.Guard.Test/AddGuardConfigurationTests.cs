using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Myth.Extensions;
using Myth.Middlewares;
using Myth.Models;
using NSubstitute;

namespace Myth.Guard.Test;

/// <summary>
/// Tests for configuration issues in AddGuard method
/// </summary>
public class AddGuardConfigurationTests {

	private readonly IWebHostEnvironment _environment;

	public AddGuardConfigurationTests( ) {
		_environment = Substitute.For<IWebHostEnvironment>( );
		_environment.EnvironmentName.Returns( Environments.Development );
	}

	[Fact]
	public async Task AddGuard_WithManualMapping_ShouldNowAutoFinalize( ) {
		// Arrange - Simulate exactly what the user did, now should work
		var services = new ServiceCollection( );
		services.AddSingleton( _environment );

		services.AddGuard( config => config
			.Guard<ArgumentNullException>( )
			.WithStatusCode( HttpStatusCode.UnprocessableEntity )
			.WithErrorCode( "INVALID" )
			.WithResponse( a => new {
				Message = a.Message,
				ParamName = a.ParamName
			} ) );
		// Note: No .Build() call here - but should work due to auto-finalization!

		var serviceProvider = services.BuildServiceProvider( );
		var options = serviceProvider.GetRequiredService<GuardOptions>( );

		// Verify handler is registered
		options.ExceptionHandlers.Should( ).ContainKey( typeof( ArgumentNullException ) );

		// Create middleware
		var next = Substitute.For<RequestDelegate>( );
		var logger = Substitute.For<ILogger<GuardExceptionMiddleware>>( );
		var middleware = new GuardExceptionMiddleware( next, options, logger );

		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var exception = new ArgumentNullException( "JAJAJAJ" );
		next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw exception );

		// Act
		await middleware.InvokeAsync( context );

		// Assert - Should work now!
		context.Response.StatusCode.Should( ).Be( ( int )HttpStatusCode.UnprocessableEntity );
	}

	[Fact]
	public async Task AddGuard_WithManualMapping_ShouldWork_WhenBuildCalled( ) {
		// Arrange - Fix the problem by calling Build()
		var services = new ServiceCollection( );
		services.AddSingleton( _environment );

		services.AddGuard( config => config
			.Guard<ArgumentNullException>( )
			.WithStatusCode( HttpStatusCode.UnprocessableEntity )
			.WithErrorCode( "INVALID" )
			.WithResponse( a => new {
				Message = a.Message,
				ParamName = a.ParamName
			} )
			.Build( ) ); // Add the missing Build() call

		var serviceProvider = services.BuildServiceProvider( );
		var options = serviceProvider.GetRequiredService<GuardOptions>( );

		// Create middleware
		var next = Substitute.For<RequestDelegate>( );
		var logger = Substitute.For<ILogger<GuardExceptionMiddleware>>( );
		var middleware = new GuardExceptionMiddleware( next, options, logger );

		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var exception = new ArgumentNullException( "JAJAJAJ" );
		next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw exception );

		// Act
		await middleware.InvokeAsync( context );

		// Assert
		context.Response.StatusCode.Should( ).Be( ( int )HttpStatusCode.UnprocessableEntity );
		context.Response.ContentType.Should( ).Contain( "application/json" );

		// Read response body
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );
		responseBody.Should( ).NotBeNullOrEmpty( );

		// Deserialize and verify response
		var responseObj = JsonSerializer.Deserialize<JsonElement>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj.GetProperty( "message" ).GetString( ).Should( ).Be( "Value cannot be null. (Parameter 'JAJAJAJ')" );
		responseObj.GetProperty( "paramName" ).GetString( ).Should( ).Be( "JAJAJAJ" );
	}

	[Fact]
	public async Task AddGuard_WithAndMethod_ShouldWork( ) {
		// Arrange - Use .And() method which should call Build() automatically
		var services = new ServiceCollection( );
		services.AddSingleton( _environment );

		services.AddGuard( config => config
			.Guard<ArgumentNullException>( )
			.WithStatusCode( HttpStatusCode.UnprocessableEntity )
			.WithErrorCode( "INVALID" )
			.WithResponse( a => new {
				Message = a.Message,
				ParamName = a.ParamName
			} )
			.And( ) ); // Use .And() instead of .Build()

		var serviceProvider = services.BuildServiceProvider( );
		var options = serviceProvider.GetRequiredService<GuardOptions>( );

		// Create middleware
		var next = Substitute.For<RequestDelegate>( );
		var logger = Substitute.For<ILogger<GuardExceptionMiddleware>>( );
		var middleware = new GuardExceptionMiddleware( next, options, logger );

		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var exception = new ArgumentNullException( "JAJAJAJ" );
		next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw exception );

		// Act
		await middleware.InvokeAsync( context );

		// Assert
		context.Response.StatusCode.Should( ).Be( ( int )HttpStatusCode.UnprocessableEntity );
		context.Response.ContentType.Should( ).Contain( "application/json" );

		// Read response body
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );
		responseBody.Should( ).NotBeNullOrEmpty( );

		// Deserialize and verify response
		var responseObj = JsonSerializer.Deserialize<JsonElement>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj.GetProperty( "message" ).GetString( ).Should( ).Be( "Value cannot be null. (Parameter 'JAJAJAJ')" );
		responseObj.GetProperty( "paramName" ).GetString( ).Should( ).Be( "JAJAJAJ" );
	}
}
