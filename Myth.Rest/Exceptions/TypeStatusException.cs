using System.Net;

namespace Myth.Exceptions {

    public class TypeStatusException : Exception {
        public HttpStatusCode StatusCode { get; private set; }

        protected TypeStatusException( ) {
        }

        public TypeStatusException( HttpStatusCode statusCode ) : base( $"Type for status code `{( int ) statusCode} {statusCode}` not found!" ) {
            StatusCode = statusCode;
        }
    }
}