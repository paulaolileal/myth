using System.Net;

namespace Myth.Extensions;

public static class StatusCodeExtensions {

	/// <summary>
	/// Check if the status code is a success
	/// </summary>
	/// <param name="statusCode">The status code</param>
	/// <returns>A value that represents the answer</returns>
	/// <remarks>
	/// Will be evaluated if the status code is greater or equal than 200
	/// and lower or equals a 299
	/// </remarks>
	public static bool IsSuccess( this HttpStatusCode statusCode ) =>
		IsSuccess( ( int )statusCode );

	/// <summary>
	/// Check if the status code is a success
	/// </summary>
	/// <param name="statusCode">The status code</param>
	/// <returns>A value that represents the answer</returns>
	/// <remarks>
	/// Will be evaluated if the status code is greater or equal than 200
	/// and lower or equals a 299
	/// </remarks>
	public static bool IsSuccess( this int statusCode ) =>
		statusCode >= 200 &&
		statusCode <= 299;
}
