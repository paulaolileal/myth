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
/// Tests to verify WithErrorCode functionality
/// </summary>
public class WithErrorCodeTests {

	[Fact]
	public async Task WithErrorCode_ShouldBeAccessibleInHandler( ) {
		// Arrange
		var services = new ServiceCollection( );

		var environment = Substitute.For<IWebHostEnvironment>( );
		environment.EnvironmentName.Returns( Environments.Development );
		services.AddSingleton( environment );

		// Store captured error code for validation
		string? capturedErrorCode = null;

		services.AddGuard( config => config
			.MapException<ArgumentNullException>( )
			.WithStatusCode( HttpStatusCode.UnprocessableEntity )
			.WithErrorCode( "INVALID_ARGUMENT" )
			.WithResponse( ex => {
				// Get the handler to check if ErrorCodeResolver is available
				var handler = config.ExceptionHandlers.GetValueOrDefault( typeof( ArgumentNullException ) );
				var errorCode = handler?.ErrorCodeResolver?.Invoke( ex );
				capturedErrorCode = errorCode;

				return new {
					Message = ex.Message,
					ParamName = ( ex as ArgumentNullException )?.ParamName,
					ErrorCode = errorCode
				};
			} ) );

		var serviceProvider = services.BuildServiceProvider( );
		var options = serviceProvider.GetRequiredService<GuardOptions>( );

		// Create middleware
		var next = Substitute.For<RequestDelegate>( );
		var logger = Substitute.For<ILogger<GuardExceptionMiddleware>>( );
		var middleware = new GuardExceptionMiddleware( next, options, logger );

		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var exception = new ArgumentNullException( "testParam" );
		next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw exception );

		// Act
		await middleware.InvokeAsync( context );

		// Assert
		capturedErrorCode.Should( ).Be( "INVALID_ARGUMENT" );

		context.Response.StatusCode.Should( ).Be( ( int )HttpStatusCode.UnprocessableEntity );

		// Read response
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );

		var responseObj = JsonSerializer.Deserialize<JsonElement>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj.GetProperty( "errorCode" ).GetString( ).Should( ).Be( "INVALID_ARGUMENT" );
	}

	[Fact]
	public async Task WithErrorCode_ShouldWorkWithDynamicResolver( ) {
		// Arrange
		var services = new ServiceCollection( );

		var environment = Substitute.For<IWebHostEnvironment>( );
		environment.EnvironmentName.Returns( Environments.Development );
		services.AddSingleton( environment );

		services.AddGuard( config => config
			.MapException<ArgumentException>( )
			.WithStatusCode( 400 )
			.WithErrorCode( ex => {
				// Dynamic error code based on parameter name
				if ( ex is ArgumentException argEx && argEx.ParamName?.ToLower( ).Contains( "email" ) == true )
					return "INVALID_EMAIL";
				if ( ex is ArgumentException argEx2 && argEx2.ParamName?.ToLower( ).Contains( "name" ) == true )
					return "INVALID_NAME";
				return "INVALID_ARGUMENT";
			} )
			.WithResponse( ex => {
				// Get the handler to access ErrorCodeResolver
				var handler = config.ExceptionHandlers.GetValueOrDefault( typeof( ArgumentException ) );
				var errorCode = handler?.ErrorCodeResolver?.Invoke( ex );

				return new {
					Message = ex.Message,
					ParamName = ( ex as ArgumentException )?.ParamName,
					ErrorCode = errorCode
				};
			} ) );

		var serviceProvider = services.BuildServiceProvider( );
		var options = serviceProvider.GetRequiredService<GuardOptions>( );

		// Create middleware
		var next = Substitute.For<RequestDelegate>( );
		var logger = Substitute.For<ILogger<GuardExceptionMiddleware>>( );
		var middleware = new GuardExceptionMiddleware( next, options, logger );

		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var exception = new ArgumentException( "Invalid email format", "userEmail" );
		next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw exception );

		// Act
		await middleware.InvokeAsync( context );

		// Assert
		context.Response.StatusCode.Should( ).Be( 400 );

		// Read response
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );

		var responseObj = JsonSerializer.Deserialize<JsonElement>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj.GetProperty( "errorCode" ).GetString( ).Should( ).Be( "INVALID_EMAIL" );
		responseObj.GetProperty( "paramName" ).GetString( ).Should( ).Be( "userEmail" );
	}

	[Fact]
	public void ErrorCodeResolver_ShouldBeRegisteredCorrectly( ) {
		// Arrange
		var options = new GuardOptions( );

		// Act
		options.MapException<InvalidOperationException>( )
			.WithErrorCode( "OPERATION_INVALID" )
			.Build( );

		// Assert
		options.ExceptionHandlers.Should( ).ContainKey( typeof( InvalidOperationException ) );
		var handler = options.ExceptionHandlers[ typeof( InvalidOperationException ) ];
		handler.ErrorCodeResolver.Should( ).NotBeNull( );
		handler.ErrorCodeResolver!.Invoke( new InvalidOperationException( "test" ) ).Should( ).Be( "OPERATION_INVALID" );
	}
}
