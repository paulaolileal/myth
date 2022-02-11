using System.Net;

namespace Myth.Exceptions {

    public class NonSuccessException : Exception {
        public HttpStatusCode StatusCode { get; private set; }
        public Uri Url { get; private set; }
        public HttpMethod Method { get; private set; }
        public string RawMessage { get; private set; }
        public Type? Type { get; private set; }
        public object? Message { get; private set; }

        protected NonSuccessException( ) {
        }

        public NonSuccessException(
            HttpStatusCode statusCode,
            Uri url,
            HttpMethod method,
            string rawMessage,
            Type? type,
            object? message )
            : base( "The request return a non sucess status code." ) {
            StatusCode = statusCode;
            Url = url;
            Method = method;
            RawMessage = rawMessage;
            Type = type;
            Message = message;
        }
    }
}