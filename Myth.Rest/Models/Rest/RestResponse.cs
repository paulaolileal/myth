using Myth.Exceptions;
using Newtonsoft.Json;
using System.Net;

namespace Myth.Models.Rest;

public class RestResponse(
	HttpStatusCode statusCode,
	Uri url,
	HttpMethod method,
	string rawMessage,
	TimeSpan elapsedTime ) : RestResponseBase( statusCode, url, method, elapsedTime ) {
	public string RawMessage { get; private set; } = rawMessage;
	public Type? ResultType { get; private set; }
	public object? Result { get; private set; }
	public dynamic DynamicResult { get; private set; } = JsonConvert.DeserializeObject<dynamic>( rawMessage )!;

	public void SetTypedResult( Type type, object result ) {
		ResultType = type;
		Result = result;
	}

	public TResult GetAs<TResult>( ) {
		if ( Result is not null && ResultType == typeof( TResult ) )
			return ( TResult )Result;

		throw new DifferentResponseTypeException( typeof( TResult ), ResultType );
	}
}