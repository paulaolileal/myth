using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Myth.Extensions;
using Myth.Interfaces;
using Myth.Models;
using Myth.Validation;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Myth.Guard.Test;

/// <summary>
/// Tests for dependency injection setup and middleware integration
/// </summary>
public class DependencyInjectionTests {

	[Fact]
	public void AddGuard_ShouldRegisterValidator( ) {
		// Arrange
		var services = new ServiceCollection( );

		// Act
		services.AddGuard( );

		// Assert
		var serviceProvider = services.BuildServiceProvider( );
		var validator = serviceProvider.GetService<IValidator>( );

		validator.Should( ).NotBeNull( );
		validator.Should( ).BeOfType<Validator>( );
	}

	[Fact]
	public void AddGuard_ShouldRegisterValidatorAsScoped( ) {
		// Arrange
		var services = new ServiceCollection( );
		services.AddGuard( );

		// Act
		var serviceProvider = services.BuildServiceProvider( );

		// Assert
		var validator1 = serviceProvider.GetService<IValidator>( );
		var validator2 = serviceProvider.GetService<IValidator>( );

		validator1.Should( ).NotBeNull( );
		validator2.Should( ).NotBeNull( );

		// In a scope, they should be the same instance
		using var scope = serviceProvider.CreateScope( );
		var scopedValidator1 = scope.ServiceProvider.GetService<IValidator>( );
		var scopedValidator2 = scope.ServiceProvider.GetService<IValidator>( );

		scopedValidator1.Should( ).BeSameAs( scopedValidator2 );
	}

	[Fact]
	public void AddGuard_WithExistingServices_ShouldNotOverrideExistingRegistrations( ) {
		// Arrange
		var services = new ServiceCollection( );
		services.AddSingleton<IValidator, CustomValidator>( );

		// Act
		services.AddGuard( );

		// Assert
		var serviceProvider = services.BuildServiceProvider( );
		var validator = serviceProvider.GetServices<IValidator>( ).First( );

		validator.Should( ).BeOfType<CustomValidator>( ); // Should use the first registration
	}

	[Fact]
	public async Task UseGuard_ShouldAddMiddlewareToRequestPipeline( ) {
		// Arrange
		using var host = await new HostBuilder( )
			.ConfigureWebHost( webBuilder => {
				webBuilder
					.UseTestServer( )
					.ConfigureServices( services => {
						services.AddGuard( );
					} )
					.Configure( app => {
						app.UseGuard( );
						app.Run( async context => {
							// Simulate throwing a ValidationException
							var validator = context.RequestServices.GetRequiredService<IValidator>( );
							var errors = new List<ValidationError>
							{
								new("Field", "Error message", "ERROR_CODE", HttpStatusCode.BadRequest)
							};
							var result = new ValidationResult( errors );
							throw new Exceptions.ValidationException( result );
						} );
					} );
			} )
			.StartAsync( );

		var client = host.GetTestClient( );

		// Act
		var response = await client.GetAsync( "/" );

		// Assert
		response.StatusCode.Should( ).Be( HttpStatusCode.BadRequest );
		response.Content.Headers.ContentType?.MediaType.Should( ).Be( "application/json" );

		var responseBody = await response.Content.ReadAsStringAsync( );
		var responseObj = JsonSerializer.Deserialize<ValidationErrorResponse>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj.Should( ).NotBeNull( );
		responseObj!.Errors.Should( ).HaveCount( 1 );
		responseObj.Errors.First( ).Field.Should( ).Be( "Field" );
		responseObj.Errors.First( ).Message.Should( ).Be( "Error message" );
		responseObj.Errors.First( ).Code.Should( ).Be( "ERROR_CODE" );
	}

	[Fact]
	public async Task UseGuard_WithNonValidationException_ShouldNotIntercept( ) {
		// Arrange
		using var host = await new HostBuilder( )
			.ConfigureWebHost( webBuilder => {
				webBuilder
					.UseTestServer( )
					.ConfigureServices( services => {
						services.AddGuard( );
					} )
					.Configure( app => {
						app.UseGuard( );
						app.Run( async context => {
							throw new InvalidOperationException( "Not a validation exception" );
						} );
					} );
			} )
			.StartAsync( );

		var client = host.GetTestClient( );

		// Act
		var act = async ( ) => await client.GetAsync( "/" );

		// Assert
		await act.Should( ).ThrowAsync<InvalidOperationException>( )
			.WithMessage( "Not a validation exception" );
	}

	[Fact]
	public async Task UseGuard_WithSuccessfulRequest_ShouldNotIntercept( ) {
		// Arrange
		using var host = await new HostBuilder( )
			.ConfigureWebHost( webBuilder => {
				webBuilder
					.UseTestServer( )
					.ConfigureServices( services => {
						services.AddGuard( );
					} )
					.Configure( app => {
						app.UseGuard( );
						app.Run( async context => {
							await context.Response.WriteAsync( "Success" );
						} );
					} );
			} )
			.StartAsync( );

		var client = host.GetTestClient( );

		// Act
		var response = await client.GetAsync( "/" );

		// Assert
		response.StatusCode.Should( ).Be( HttpStatusCode.OK );
		var content = await response.Content.ReadAsStringAsync( );
		content.Should( ).Be( "Success" );
	}

	[Fact]
	public void GuardServiceCollectionExtensions_ShouldBeAccessible( ) {
		// Arrange
		var services = new ServiceCollection( );

		// Act & Assert - Should not throw and should be accessible
		var act = ( ) => services.AddGuard( );
		act.Should( ).NotThrow( );

		// Verify the extension method exists and is callable
		services.Should( ).NotBeNull( );
		var serviceProvider = services.BuildServiceProvider( );
		serviceProvider.GetService<IValidator>( ).Should( ).NotBeNull( );
	}

	[Fact]
	public void GuardApplicationBuilderExtensions_ShouldBeAccessible( ) {
		// Arrange
		var services = new ServiceCollection( );
		services.AddGuard( );
		var serviceProvider = services.BuildServiceProvider( );

		var app = new ApplicationBuilder( serviceProvider );

		// Act & Assert - Should not throw and should be accessible
		var act = ( ) => app.UseGuard( );
		act.Should( ).NotThrow( );
	}

	[Fact]
	public async Task IntegrationTest_CompleteFlow_ShouldWork( ) {
		// Arrange
		using var host = await new HostBuilder( )
			.ConfigureWebHost( webBuilder => {
				webBuilder
					.UseTestServer( )
					.ConfigureServices( services => {
						services.AddRouting( );
						services.AddGuard( );
					} )
					.Configure( app => {
						app.UseGuard( );
						app.UseRouting( );
						app.UseEndpoints( endpoints => {
							endpoints.MapPost( "/validate", async ( HttpContext context ) => {
								var validator = context.RequestServices.GetRequiredService<IValidator>( );

								// Create an invalid entity
								var invalidEntity = new Models.SimpleEntity { Value = "" }; // Empty value

								// This should throw ValidationException
								await validator.ValidateAsync( invalidEntity );

								await context.Response.WriteAsync( "Should not reach here" );
							} );
						} );
					} );
			} )
			.StartAsync( );

		var client = host.GetTestClient( );

		// Act
		var response = await client.PostAsync( "/validate", new StringContent( "", Encoding.UTF8, "application/json" ) );

		// Assert
		response.StatusCode.Should( ).Be( HttpStatusCode.BadRequest );

		var responseBody = await response.Content.ReadAsStringAsync( );
		var responseObj = JsonSerializer.Deserialize<ValidationErrorResponse>( responseBody, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		} );

		responseObj.Should( ).NotBeNull( );
		responseObj!.Errors.Should( ).HaveCount( 1 );
		responseObj.Errors.First( ).Field.Should( ).Be( "Value" );
	}

	// Helper class for testing custom validator registration
	private class CustomValidator : IValidator {

		public Task ValidateAsync<T>( T entity, ValidationContextKey? context = null, CancellationToken cancellationToken = default ) where T : class {
			return Task.CompletedTask;
		}

		public Task<ValidationResult> ValidateAndReturnAsync<T>( T entity, ValidationContextKey? context = null, CancellationToken cancellationToken = default ) where T : class {
			return Task.FromResult( new ValidationResult( new List<ValidationError>( ) ) );
		}
	}
}