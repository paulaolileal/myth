using Myth.Extensions;
using System.Net;

namespace Myth.Models.Rest {

	public abstract class RestResponseBase {
		public HttpStatusCode StatusCode { get; private set; }
		public Uri Url { get; private set; }
		public HttpMethod Method { get; private set; }
		public TimeSpan ElapsedTime { get; private set; }

		public RestResponseBase(
			HttpStatusCode statusCode,
			Uri url,
			HttpMethod method,
			TimeSpan elapsedTime ) {
			StatusCode = statusCode;
			Url = url;
			Method = method;
		}

		public bool IsSuccessStatusCode( ) => StatusCode.IsSuccess( );
	}
}