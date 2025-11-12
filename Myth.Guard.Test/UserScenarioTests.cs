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
/// Tests that reproduce the exact user scenario
/// </summary>
public class UserScenarioTests {

	[Fact]
	public async Task ExactUserScenario_ShouldNowWork_AfterFix( ) {
		// Arrange - Exactly what the user did, but now should work due to auto-finalization
		var services = new ServiceCollection( );

		var environment = Substitute.For<IWebHostEnvironment>( );
		environment.EnvironmentName.Returns( Environments.Development );
		services.AddSingleton( environment );

		// Exactly the user's configuration - no .Build() or .And() call
		services.AddGuard( config => config
			.GuardException<ArgumentNullException>( )
			.WithStatusCode( HttpStatusCode.UnprocessableEntity )
			.WithErrorCode( "INVALID" )
			.WithResponse( a => new {
				Message = a.Message,
				ParamName = a.ParamName
			} ) );

		var serviceProvider = services.BuildServiceProvider( );
		var options = serviceProvider.GetRequiredService<GuardOptions>( );

		// Check if the handler is now registered automatically
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

		// Assert - Should now work!
		context.Response.StatusCode.Should( ).Be( ( int )HttpStatusCode.UnprocessableEntity );
		context.Response.ContentType.Should( ).Contain( "application/json" );

		// Read response
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );
		responseBody.Should( ).NotBeNullOrEmpty( );

		var responseObj = JsonSerializer.Deserialize<JsonElement>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj.GetProperty( "message" ).GetString( ).Should( ).Be( "Value cannot be null. (Parameter 'JAJAJAJ')" );
		responseObj.GetProperty( "paramName" ).GetString( ).Should( ).Be( "JAJAJAJ" );
	}

	[Fact]
	public async Task UserScenario_WithProperConfiguration_ShouldWork( ) {
		// Arrange - The CORRECT way to configure
		var services = new ServiceCollection( );

		var environment = Substitute.For<IWebHostEnvironment>( );
		environment.EnvironmentName.Returns( Environments.Development );
		services.AddSingleton( environment );

		// Use .And() to auto-build the handler
		services.AddGuard( config => config
			.GuardException<ArgumentNullException>( )
			.WithStatusCode( HttpStatusCode.UnprocessableEntity )
			.WithErrorCode( "INVALID" )
			.WithResponse( a => new {
				Message = a.Message,
				ParamName = a.ParamName,
				ErrorCode = "INVALID" // Include error code in response manually for now
			} )
			.And( ) ); // This should call Build() automatically

		var serviceProvider = services.BuildServiceProvider( );
		var options = serviceProvider.GetRequiredService<GuardOptions>( );

		// Check if the handler is actually registered now
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

		// Assert
		context.Response.StatusCode.Should( ).Be( ( int )HttpStatusCode.UnprocessableEntity );
		context.Response.ContentType.Should( ).Contain( "application/json" );

		// Read response
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );
		responseBody.Should( ).NotBeNullOrEmpty( );

		var responseObj = JsonSerializer.Deserialize<JsonElement>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj.GetProperty( "message" ).GetString( ).Should( ).Be( "Value cannot be null. (Parameter 'JAJAJAJ')" );
		responseObj.GetProperty( "paramName" ).GetString( ).Should( ).Be( "JAJAJAJ" );
		responseObj.GetProperty( "errorCode" ).GetString( ).Should( ).Be( "INVALID" );
	}
}
