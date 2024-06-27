using Myth.Models.Rest;
using System.Net;

namespace Myth.Exceptions;

public class NonSuccessException(
	HttpStatusCode statusCode,
	Uri url,
	HttpMethod method,
	string rawMessage,
	object? result ) : Exception( "The request return a non sucess status code." ) {
	public HttpStatusCode StatusCode { get; private set; } = statusCode;
	public Uri Url { get; private set; } = url;
	public HttpMethod Method { get; private set; } = method;
	public string RawMessage { get; private set; } = rawMessage;
	public object? Result { get; private set; } = result;

	public NonSuccessException( RestResponse response )
		: this(
		response.StatusCode,
		response.Url,
		response.Method,
		response.RawMessage,
		response.Result ) { }

	public NonSuccessException( RestFileResponse response )
		: this(
		response.StatusCode,
		response.Url,
		response.Method,
		"File",
		null ) { }
}