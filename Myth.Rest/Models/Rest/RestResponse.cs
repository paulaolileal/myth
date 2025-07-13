using Myth.Exceptions;
using Newtonsoft.Json;
using System.Net;

namespace Myth.Models.Rest;

public class RestResponse(
		HttpStatusCode statusCode,
		Uri url,
		HttpMethod method,
		string rawMessage,
		TimeSpan elapsedTime,
		int retriesMade,
		bool fallbackUsed )
	: RestResponseBase( statusCode, url, method, elapsedTime, retriesMade, fallbackUsed ) {
	public string RawMessage { get; private set; } = rawMessage;
	public Type? ResultType { get; private set; }
	public object? Result { get; private set; }
	public dynamic DynamicResult { get; private set; } = JsonConvert.DeserializeObject<dynamic>( rawMessage )!;

	internal void SetTypedResult( Type type, object result ) {
		ResultType = type;
		Result = result;
	}

	/// <summary>
	/// Get the result typed with the mapped type
	/// </summary>
	/// <typeparam name="TResult">The mapped type</typeparam>
	/// <returns>The strongly typed object</returns>
	/// <exception cref="DifferentResponseTypeException">Throws when the <c>TResult</c> is different from mapped type</exception>
	public TResult GetAs<TResult>( ) {
		if ( Result is not null && ResultType == typeof( TResult ) )
			return ( TResult )Result;

		throw new DifferentResponseTypeException( typeof( TResult ), ResultType );
	}
}