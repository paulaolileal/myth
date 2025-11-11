using System.Net;

namespace Myth.Testing.Mocks;

/// <summary>
/// Fluent builder for configuring mock HTTP endpoints
/// </summary>
/// <remarks>
/// Provides a fluent API for setting up mock HTTP responses including route, method, status code, and response body.
/// </remarks>
public class MockEndpointBuilder {
	internal HttpMethod Method { get; set; } = HttpMethod.Get;
	internal object? Response { get; set; }
	internal string Route { get; set; } = "/";
	internal HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

	/// <summary>
	/// Configure the endpoint to respond to POST requests
	/// </summary>
	/// <returns>The builder instance for fluent chaining</returns>
	public MockEndpointBuilder UsingPost( ) {
		Method = HttpMethod.Post;
		return this;
	}

	/// <summary>
	/// Configure the endpoint to respond to GET requests
	/// </summary>
	/// <returns>The builder instance for fluent chaining</returns>
	public MockEndpointBuilder UsingGet( ) {
		Method = HttpMethod.Get;
		return this;
	}

	/// <summary>
	/// Configure the endpoint to respond to PUT requests
	/// </summary>
	/// <returns>The builder instance for fluent chaining</returns>
	public MockEndpointBuilder UsingPut( ) {
		Method = HttpMethod.Put;
		return this;
	}

	/// <summary>
	/// Configure the endpoint to respond to DELETE requests
	/// </summary>
	/// <returns>The builder instance for fluent chaining</returns>
	public MockEndpointBuilder UsingDelete( ) {
		Method = HttpMethod.Delete;
		return this;
	}

	/// <summary>
	/// Configure the endpoint to respond to PATCH requests
	/// </summary>
	/// <returns>The builder instance for fluent chaining</returns>
	public MockEndpointBuilder UsingPatch( ) {
		Method = HttpMethod.Patch;
		return this;
	}

	/// <summary>
	/// Configure the endpoint to respond to a specific HTTP method
	/// </summary>
	/// <param name="method">The HTTP method</param>
	/// <returns>The builder instance for fluent chaining</returns>
	public MockEndpointBuilder UsingMethod( HttpMethod method ) {
		Method = method;
		return this;
	}

	/// <summary>
	/// Set the JSON response body for the endpoint
	/// </summary>
	/// <typeparam name="T">The type of the response object</typeparam>
	/// <param name="response">The response object that will be serialized to JSON</param>
	/// <returns>The builder instance for fluent chaining</returns>
	public MockEndpointBuilder WithJsonResponse<T>( T response ) {
		Response = response;
		return this;
	}

	/// <summary>
	/// Set the route pattern for the endpoint
	/// </summary>
	/// <param name="route">The route pattern (e.g., "/api/users/{id}")</param>
	/// <returns>The builder instance for fluent chaining</returns>
	public MockEndpointBuilder ForRoute( string route ) {
		Route = route;
		return this;
	}

	/// <summary>
	/// Set the HTTP status code for the response
	/// </summary>
	/// <param name="statusCode">The HTTP status code</param>
	/// <returns>The builder instance for fluent chaining</returns>
	public MockEndpointBuilder RespondWith( HttpStatusCode statusCode ) {
		StatusCode = statusCode;
		return this;
	}

	/// <summary>
	/// Configure the endpoint to return a successful response (200 OK)
	/// </summary>
	/// <returns>The builder instance for fluent chaining</returns>
	public MockEndpointBuilder RespondWithSuccess( ) {
		StatusCode = HttpStatusCode.OK;
		return this;
	}

	/// <summary>
	/// Configure the endpoint to return a not found response (404 Not Found)
	/// </summary>
	/// <returns>The builder instance for fluent chaining</returns>
	public MockEndpointBuilder RespondWithNotFound( ) {
		StatusCode = HttpStatusCode.NotFound;
		return this;
	}

	/// <summary>
	/// Configure the endpoint to return a server error response (500 Internal Server Error)
	/// </summary>
	/// <returns>The builder instance for fluent chaining</returns>
	public MockEndpointBuilder RespondWithServerError( ) {
		StatusCode = HttpStatusCode.InternalServerError;
		return this;
	}

	/// <summary>
	/// Configure the endpoint to return a bad request response (400 Bad Request)
	/// </summary>
	/// <returns>The builder instance for fluent chaining</returns>
	public MockEndpointBuilder RespondWithBadRequest( ) {
		StatusCode = HttpStatusCode.BadRequest;
		return this;
	}

	/// <summary>
	/// Configure the endpoint to return an unauthorized response (401 Unauthorized)
	/// </summary>
	/// <returns>The builder instance for fluent chaining</returns>
	public MockEndpointBuilder RespondWithUnauthorized( ) {
		StatusCode = HttpStatusCode.Unauthorized;
		return this;
	}
}
