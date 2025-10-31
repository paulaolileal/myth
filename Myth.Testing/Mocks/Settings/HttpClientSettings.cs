using System.Net;
using System.Net.Http;

namespace Myth.Rest.Test.Base {

	public class HttpClientSettings {
		internal HttpMethod Method { get; set; } = HttpMethod.Get;
		internal object? Response { get; set; }
		internal string Route { get; set; } = null!;
		internal HttpStatusCode StatusCode { get; set; }

		public HttpClientSettings UsePost( ) {
			Method = HttpMethod.Post;
			return this;
		}

		public HttpClientSettings UseGet( ) {
			Method = HttpMethod.Get;
			return this;
		}

		public HttpClientSettings UsePut( ) {
			Method = HttpMethod.Put;
			return this;
		}

		public HttpClientSettings UseDelete( ) {
			Method = HttpMethod.Delete;
			return this;
		}

		public HttpClientSettings UsePatch( ) {
			Method = HttpMethod.Patch;
			return this;
		}

		public HttpClientSettings UseResponse<T>( T response ) {
			Response = response;
			return this;
		}

		public HttpClientSettings UseRoute( string route ) {
			Route = route;
			return this;
		}

		public HttpClientSettings UseStatusCode( HttpStatusCode statusCode ) {
			StatusCode = statusCode;
			return this;
		}
	}
}