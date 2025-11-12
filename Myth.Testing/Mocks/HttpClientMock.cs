using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Myth.Extensions;

namespace Myth.Testing.Mocks;

/// <summary>
/// Mock HTTP client factory for testing HTTP-based services
/// </summary>
/// <remarks>
/// Creates a test server with configurable endpoints for mocking external HTTP dependencies.
/// Useful for integration tests that need to mock external APIs or services.
/// </remarks>
public class HttpClientMock {

	/// <summary>
	/// Creates a mock HTTP client with a test server configured according to the provided settings
	/// </summary>
	/// <param name="configure">Configuration action for setting up the mock endpoint</param>
	/// <returns>An HttpClient instance backed by a test server</returns>
	/// <example>
	/// <code>
	/// var client = HttpClientMock.CreateClient(config => config
	///     .ForRoute("/api/users")
	///     .RespondWith(HttpStatusCode.OK)
	///     .WithJsonResponse(new { Id = 1, Name = "John Doe" })
	///     .UsingMethod(HttpMethod.Get));
	/// </code>
	/// </example>
	public static HttpClient CreateClient( Action<MockEndpointBuilder> configure ) {
		var mockSettings = new MockEndpointBuilder( );
		configure.Invoke( mockSettings );

		var builder = new WebHostBuilder( )
			.ConfigureServices( services => services.AddRouting( ) )
			.Configure( app => {
				app.UseRouting( );

				app.UseEndpoints( endpoints => {
					RequestDelegate handler = async context => {
						context.Response.StatusCode = ( int )mockSettings.StatusCode;

						if ( mockSettings.Response is not null ) {
							// Check if it's a string response with custom content type
							if ( mockSettings.Response.GetType( ).GetProperty( "Content" ) is not null &&
								 mockSettings.Response.GetType( ).GetProperty( "ContentType" ) is not null ) {
								var content = mockSettings.Response.GetType( ).GetProperty( "Content" )?.GetValue( mockSettings.Response ) as string;
								var contentType = mockSettings.Response.GetType( ).GetProperty( "ContentType" )?.GetValue( mockSettings.Response ) as string;

								context.Response.ContentType = contentType ?? "text/plain";
								await context.Response.WriteAsync( content ?? "" );
							} else {
								// Default JSON response
								context.Response.ContentType = "application/json";
								await context.Response.WriteAsync( mockSettings.Response.ToJson( ) );
							}
						}
					};

					var route = mockSettings.Route.ToLowerInvariant( );
					var method = mockSettings.Method.Method.ToUpperInvariant( );

					switch ( mockSettings.Method.Method ) {
						case "GET":
							endpoints.MapGet( route, handler );
							break;

						case "POST":
							endpoints.MapPost( route, handler );
							break;

						case "PUT":
							endpoints.MapPut( route, handler );
							break;

						case "DELETE":
							endpoints.MapDelete( route, handler );
							break;

						case "PATCH":
							endpoints.MapPatch( route, handler );
							break;

						default:
							throw new NotSupportedException( $"HTTP method '{method}' is not supported." );
					}
				} );
			} );

		var testServer = new TestServer( builder );

		return testServer.CreateClient( );
	}

	/// <summary>
	/// Creates multiple mock HTTP clients with different endpoint configurations
	/// </summary>
	/// <param name="endpoints">Array of endpoint configurations</param>
	/// <returns>An HttpClient instance backed by a test server with multiple endpoints</returns>
	/// <example>
	/// <code>
	/// var client = HttpClientMock.CreateClientWithEndpoints(
	///     config => config.ForRoute("/api/users").UsingGet().RespondWithSuccess(),
	///     config => config.ForRoute("/api/users").UsingPost().RespondWith(HttpStatusCode.Created)
	/// );
	/// </code>
	/// </example>
	public static HttpClient CreateClientWithEndpoints( params Action<MockEndpointBuilder>[ ] endpoints ) {
		var builder = new WebHostBuilder( )
			.ConfigureServices( services => services.AddRouting( ) )
			.Configure( app => {
				app.UseRouting( );

				app.UseEndpoints( routeBuilder => {
					foreach ( var endpointConfig in endpoints ) {
						var mockSettings = new MockEndpointBuilder( );
						endpointConfig.Invoke( mockSettings );

						RequestDelegate handler = async context => {
							context.Response.StatusCode = ( int )mockSettings.StatusCode;

							if ( mockSettings.Response is not null ) {
								// Check if it's a string response with custom content type
								if ( mockSettings.Response.GetType( ).GetProperty( "Content" ) is not null &&
									 mockSettings.Response.GetType( ).GetProperty( "ContentType" ) is not null ) {
									var content = mockSettings.Response.GetType( ).GetProperty( "Content" )?.GetValue( mockSettings.Response ) as string;
									var contentType = mockSettings.Response.GetType( ).GetProperty( "ContentType" )?.GetValue( mockSettings.Response ) as string;

									context.Response.ContentType = contentType ?? "text/plain";
									await context.Response.WriteAsync( content ?? "" );
								} else {
									// Default JSON response
									context.Response.ContentType = "application/json";
									await context.Response.WriteAsync( mockSettings.Response.ToJson( ) );
								}
							}
						};

						var route = mockSettings.Route.ToLowerInvariant( );

						switch ( mockSettings.Method.Method ) {
							case "GET":
								routeBuilder.MapGet( route, handler );
								break;

							case "POST":
								routeBuilder.MapPost( route, handler );
								break;

							case "PUT":
								routeBuilder.MapPut( route, handler );
								break;

							case "DELETE":
								routeBuilder.MapDelete( route, handler );
								break;

							case "PATCH":
								routeBuilder.MapPatch( route, handler );
								break;

							default:
								throw new NotSupportedException( $"HTTP method '{mockSettings.Method.Method}' is not supported." );
						}
					}
				} );
			} );

		var testServer = new TestServer( builder );

		return testServer.CreateClient( );
	}
}
