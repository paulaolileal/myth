using System.Net;
using FluentAssertions;
using FluentAssertions.Primitives;
using Microsoft.AspNetCore.Mvc;

namespace Myth.Extensions;

public static class FluentAssertionExtensions {

	/// <summary>
	/// Asserts if the http status is the expected status
	/// </summary>
	/// <typeparam name="T">The status content type expected</typeparam>
	/// <param name="assertions">The fluent assertion</param>
	/// <param name="statusCode">The status code expected</param>
	public static void BeStatusCode<T>( this ObjectAssertions assertions, HttpStatusCode statusCode ) where T : StatusCodeResult {
		assertions.Subject
			.As<T>( )
			.StatusCode
			.Should( )
			.Be( ( int )statusCode );
	}

	/// <summary>
	/// Asserts if the http status is the expected status
	/// </summary>
	/// <typeparam name="T">The status content type expected</typeparam>
	/// <param name="assertions">The fluent assertion</param>
	/// <param name="statusCode">The status code expected</param>
	public static void BeObjectResultStatusCode<T>( this ObjectAssertions assertions, HttpStatusCode statusCode ) where T : ObjectResult {
		assertions.Subject
			.As<T>( )
			.StatusCode
			.Should( )
			.Be( ( int )statusCode );
	}

	/// <summary>
	/// Asserts if the http status is the expected status
	/// </summary>
	/// <typeparam name="T">The status content type expected</typeparam>
	/// <param name="assertions">The fluent assertion</param>
	/// <param name="statusCode">The status code expected</param>
	public static void BeContentResult( this ObjectAssertions assertions, HttpStatusCode statusCode ) {
		assertions.Subject
			.As<ContentResult>( )
			.StatusCode
			.Should( )
			.Be( ( int )statusCode );
	}

	/// <summary>
	/// Asserts if the http status code is <c>204 No Content</c>
	/// </summary>
	/// <param name="assertions">The fluent assertion</param>
	public static void BeStatusCodeNoContent( this ObjectAssertions assertions ) {
		assertions.BeStatusCode<NoContentResult>( HttpStatusCode.NoContent );
	}

	/// <summary>
	/// Asserts if the http status code is <c>200 Ok</c>
	/// </summary>
	/// <param name="assertions">The fluent assertion</param>
	public static void BeStatusCodeOk( this ObjectAssertions assertions ) {
		if ( assertions.Subject is StatusCodeResult )
			assertions.BeStatusCode<OkResult>( HttpStatusCode.OK );
		else
			assertions.BeObjectResultStatusCode<OkObjectResult>( HttpStatusCode.OK );
	}

	/// <summary>
	/// Asserts if the http status code is <c>422 Unprocessable entity</c>
	/// </summary>
	/// <param name="assertions">The fluent assertion</param>
	public static void BeStatusCodeUnprocessableEntity( this ObjectAssertions assertions ) {
		if ( assertions.Subject is StatusCodeResult )
			assertions.BeStatusCode<UnprocessableEntityResult>( HttpStatusCode.UnprocessableEntity );
		else
			assertions.BeObjectResultStatusCode<UnprocessableEntityObjectResult>( HttpStatusCode.OK );
	}

	/// <summary>
	/// Asserts if the http status code is <c>201 Created</c>
	/// </summary>
	/// <param name="assertions">The fluent assertion</param>
	public static void BeStatusCodeCreated( this ObjectAssertions assertions ) {
		if ( assertions.Subject is ContentResult )
			assertions.BeContentResult( HttpStatusCode.Created );
		else
			assertions.BeObjectResultStatusCode<CreatedResult>( HttpStatusCode.Created );
	}
}
