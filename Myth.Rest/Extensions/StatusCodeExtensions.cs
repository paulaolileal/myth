using System.Net;

namespace Myth.Extensions {

	public static class StatusCodeExtensions {

		public static bool IsSuccess( this HttpStatusCode statusCode ) {
			return IsSuccess( ( int )statusCode );
		}

		public static bool IsSuccess( this int statusCode ) {
			return statusCode >= 200 &&
				   statusCode <= 299;
		}
	}
}