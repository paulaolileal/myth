using Myth.Extensions;
using System.Net;

namespace Myth.Models.Rest;

public abstract class RestResponseBase(
	HttpStatusCode statusCode,
	Uri url,
	HttpMethod method,
	TimeSpan elapsedTime ) {

	/// <summary>
	/// The response status code
	/// </summary>
	public HttpStatusCode StatusCode { get; private set; } = statusCode;

	/// <summary>
	/// The url called
	/// </summary>
	public Uri Url { get; private set; } = url;

	/// <summary>
	/// The http method used
	/// </summary>
	public HttpMethod Method { get; private set; } = method;

	/// <summary>
	/// The elapsed time during the call
	/// </summary>
	public TimeSpan ElapsedTime { get; private set; } = elapsedTime;

	/// <summary>
	/// Checks if the result is a success
	/// </summary>
	/// <returns>A value that represents the answer</returns>
	public bool IsSuccessStatusCode( ) => StatusCode.IsSuccess( );
}