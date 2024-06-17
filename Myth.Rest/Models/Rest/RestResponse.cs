using Myth.Exceptions;
using Newtonsoft.Json;
using System.Net;

namespace Myth.Models.Rest {

	public class RestResponse : RestResponseBase {
		public string RawMessage { get; private set; }
		public Type? ResultType { get; private set; }
		public object? Result { get; private set; }
		public dynamic DynamicResult { get; private set; }

		public RestResponse(
			HttpStatusCode statusCode,
			Uri url,
			HttpMethod method,
			string rawMessage,
			TimeSpan elapsedTime )
			: base( statusCode, url, method, elapsedTime ) {
			RawMessage = rawMessage;
			DynamicResult = JsonConvert.DeserializeObject<dynamic>( rawMessage )!;
		}

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
}