using Myth.Models.Rest;
using System.Net;

namespace Myth.Exceptions {

	public class NonSuccessException : Exception {
		public HttpStatusCode StatusCode { get; private set; }
		public Uri Url { get; private set; } = null!;
		public HttpMethod Method { get; private set; } = null!;
		public string RawMessage { get; private set; } = null!;
		public object? Result { get; private set; }

		public NonSuccessException(
			HttpStatusCode statusCode,
			Uri url,
			HttpMethod method,
			string rawMessage,
			object? result )
			: base( "The request return a non sucess status code." ) {
			StatusCode = statusCode;
			Url = url;
			Method = method;
			RawMessage = rawMessage;
			Result = result;
		}

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
}