using Myth.Extensions;
using System.Net;

namespace Myth.Models.Rest;

public abstract class RestResponseBase(
	HttpStatusCode statusCode,
	Uri url,
	HttpMethod method,
	TimeSpan elapsedTime ) {
	public HttpStatusCode StatusCode { get; private set; } = statusCode;
	public Uri Url { get; private set; } = url;
	public HttpMethod Method { get; private set; } = method;
	public TimeSpan ElapsedTime { get; private set; } = elapsedTime;

	public bool IsSuccessStatusCode( ) => StatusCode.IsSuccess( );
}