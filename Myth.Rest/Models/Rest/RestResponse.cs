using Myth.Exceptions;
using System.Net;

namespace Myth.Models.Rest {

    public class RestResponse {
        public HttpStatusCode StatusCode { get; private set; }
        public Uri Url { get; private set; }
        public HttpMethod Method { get; private set; }
        public string RawMessage { get; private set; }
        public Type? ResultType { get; private set; }
        public object? Result { get; private set; }

        public RestResponse(
            HttpStatusCode statusCode,
            Uri url,
            HttpMethod method,
            string rawMessage,
            Type? resultType,
            object? message ) {
            StatusCode = statusCode;
            Url = url;
            Method = method;
            RawMessage = rawMessage;
            ResultType = resultType;
            Result = message;
        }

        public TResult GetAs<TResult>( ) {
            if ( Result is not null && typeof( TResult ) == ResultType )
                return ( TResult ) Result;            

            throw new ResponseTypeException( typeof( TResult ), ResultType );
        }
    }
}