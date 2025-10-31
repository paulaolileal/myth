using System.Net;
using System.Net.Http;

namespace Myth.Rest.Test.Base {

	internal class TestServerSettings {
		internal HttpMethod Method { get; set; } = HttpMethod.Get;
		internal object? Response { get; set; }
		internal string Route { get; set; } = null!;
		internal HttpStatusCode StatusCode { get; set; }

		public TestServerSettings UsePost( ) {
			Method = HttpMethod.Post;
			return this;
		}

		public TestServerSettings UseGet( ) {
			Method = HttpMethod.Get;
			return this;
		}

		public TestServerSettings UsePut( ) {
			Method = HttpMethod.Put;
			return this;
		}

		public TestServerSettings UseDelete( ) {
			Method = HttpMethod.Delete;
			return this;
		}

		public TestServerSettings UsePatch( ) {
			Method = HttpMethod.Patch;
			return this;
		}

		public TestServerSettings UseResponse<T>( T response ) {
			Response = response;
			return this;
		}

		public TestServerSettings UseRoute( string route ) {
			Route = route;
			return this;
		}

		public TestServerSettings UseStatusCode( HttpStatusCode statusCode ) {
			StatusCode = statusCode;
			return this;
		}
	}
}