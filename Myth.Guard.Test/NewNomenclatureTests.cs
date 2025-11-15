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
/// Tests demonstrating the new Guard* nomenclature
/// </summary>
public class NewNomenclatureTests {

	private readonly IWebHostEnvironment _environment;

	public NewNomenclatureTests( ) {
		_environment = Substitute.For<IWebHostEnvironment>( );
		_environment.EnvironmentName.Returns( Environments.Development );
	}

	[Fact]
	public async Task GuardException_ShouldWork( ) {
		// Arrange - Using new Guard nomenclature
		var services = new ServiceCollection( );
		services.AddSingleton( _environment );

		services.AddGuard( config => config
			.GuardException<ArgumentNullException>( )
			.WithStatusCode( HttpStatusCode.BadRequest )
			.WithErrorCode( "NULL_ARGUMENT" )
			.WithResponse( ex => new {
				error = "Argument cannot be null",
				parameter = ex.ParamName,
				message = ex.Message
			} ) );

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

		var exception = new ArgumentNullException( "userId", "User ID cannot be null" );
		next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw exception );

		// Act
		await middleware.InvokeAsync( context );

		// Assert
		context.Response.StatusCode.Should( ).Be( 400 );
		context.Response.ContentType.Should( ).Contain( "application/json" );

		// Read response
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );

		var responseObj = JsonSerializer.Deserialize<JsonElement>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj.GetProperty( "error" ).GetString( ).Should( ).Be( "Argument cannot be null" );
		responseObj.GetProperty( "parameter" ).GetString( ).Should( ).Be( "userId" );
		responseObj.GetProperty( "message" ).GetString( ).Should( ).Be( "User ID cannot be null (Parameter 'userId')" );
	}

	[Fact]
	public async Task GuardDefaultException_ShouldWork( ) {
		// Arrange - Using new Guard nomenclature
		var services = new ServiceCollection( );
		services.AddSingleton( _environment );

		services.AddGuard( config => config
			.GuardDefaultException( )
			.WithStatusCode( 503 )
			.WithErrorCode( "SERVICE_UNAVAILABLE" )
			.WithResponse( ex => new {
				error = "Service temporarily unavailable",
				type = ex.GetType( ).Name,
				details = ex.Message
			} ) );

		var serviceProvider = services.BuildServiceProvider( );
		var options = serviceProvider.GetRequiredService<GuardOptions>( );

		// Create middleware
		var next = Substitute.For<RequestDelegate>( );
		var logger = Substitute.For<ILogger<GuardExceptionMiddleware>>( );
		var middleware = new GuardExceptionMiddleware( next, options, logger );

		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var exception = new InvalidOperationException( "Database connection failed" );
		next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw exception );

		// Act
		await middleware.InvokeAsync( context );

		// Assert
		context.Response.StatusCode.Should( ).Be( 503 );
		context.Response.ContentType.Should( ).Contain( "application/json" );

		// Read response
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );

		var responseObj = JsonSerializer.Deserialize<JsonElement>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj.GetProperty( "error" ).GetString( ).Should( ).Be( "Service temporarily unavailable" );
		responseObj.GetProperty( "type" ).GetString( ).Should( ).Be( "InvalidOperationException" );
		responseObj.GetProperty( "details" ).GetString( ).Should( ).Be( "Database connection failed" );
	}

	[Fact]
	public async Task AutoGuardCommonExceptions_ShouldWork( ) {
		// Arrange - Using new Guard nomenclature
		var services = new ServiceCollection( );
		services.AddSingleton( _environment );

		services.AddGuard( config => config
			.AutoGuardCommonExceptions( ) );

		var serviceProvider = services.BuildServiceProvider( );
		var options = serviceProvider.GetRequiredService<GuardOptions>( );

		// Verify common handlers are registered
		options.ExceptionHandlers.Should( ).ContainKey( typeof( ArgumentNullException ) );
		options.ExceptionHandlers.Should( ).ContainKey( typeof( ArgumentException ) );
		options.ExceptionHandlers.Should( ).ContainKey( typeof( InvalidOperationException ) );

		// Create middleware
		var next = Substitute.For<RequestDelegate>( );
		var logger = Substitute.For<ILogger<GuardExceptionMiddleware>>( );
		var middleware = new GuardExceptionMiddleware( next, options, logger );

		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var exception = new ArgumentNullException( "testParam", "Test null parameter" );
		next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw exception );

		// Act
		await middleware.InvokeAsync( context );

		// Assert
		context.Response.StatusCode.Should( ).Be( 400 );
		context.Response.ContentType.Should( ).Contain( "application/json" );

		// Read response
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );

		var responseObj = JsonSerializer.Deserialize<JsonElement>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj.GetProperty( "error" ).GetString( ).Should( ).Be( "Test null parameter (Parameter 'testParam')" );
		responseObj.GetProperty( "parameter" ).GetString( ).Should( ).Be( "testParam" );
	}

	[Fact]
	public async Task MixedNewAndOldNomenclature_ShouldWork( ) {
		// Arrange - Mixing new and old nomenclature for backward compatibility
		var services = new ServiceCollection( );
		services.AddSingleton( _environment );

		services.AddGuard( config => config
			.AutoGuardCommonExceptions( ) // New nomenclature
			.GuardException<FileNotFoundException>( ) // New nomenclature
			.WithStatusCode( 404 )
			.WithResponse( ex => new { error = "File not found", fileName = ex.FileName } ) );

		var serviceProvider = services.BuildServiceProvider( );
		var options = serviceProvider.GetRequiredService<GuardOptions>( );

		// Verify both handlers are registered
		options.ExceptionHandlers.Should( ).ContainKey( typeof( ArgumentNullException ) ); // From AutoGuard
		options.ExceptionHandlers.Should( ).ContainKey( typeof( FileNotFoundException ) ); // From Guard

		// Create middleware
		var next = Substitute.For<RequestDelegate>( );
		var logger = Substitute.For<ILogger<GuardExceptionMiddleware>>( );
		var middleware = new GuardExceptionMiddleware( next, options, logger );

		var context = new DefaultHttpContext( );
		context.Response.Body = new MemoryStream( );

		var exception = new FileNotFoundException( "Config file not found", "config.json" );
		next.When( x => x( Arg.Any<HttpContext>( ) ) ).Do( x => throw exception );

		// Act
		await middleware.InvokeAsync( context );

		// Assert
		context.Response.StatusCode.Should( ).Be( 404 );
		context.Response.ContentType.Should( ).Contain( "application/json" );

		// Read response
		context.Response.Body.Seek( 0, SeekOrigin.Begin );
		var responseBody = await new StreamReader( context.Response.Body ).ReadToEndAsync( );

		var responseObj = JsonSerializer.Deserialize<JsonElement>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj.GetProperty( "error" ).GetString( ).Should( ).Be( "File not found" );
		responseObj.GetProperty( "fileName" ).GetString( ).Should( ).Be( "config.json" );
	}
}
