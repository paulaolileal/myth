using System;
using System.Net;

namespace Myth.Exceptions {

    public class RequestException : Exception {

        public int? StatusCode { get; set; }

        public string Url { get; set; }

        public string Content { get; set; }

        protected RequestException( ) {
        }

        public RequestException( int? statusCode, string url, string content, string message ) : base( message ) {
            StatusCode = statusCode;
            Url = url;
            Content = content;
        }

        public RequestException( HttpStatusCode? statusCode, Uri url, string content, string message ) : base( message ) {
            StatusCode = ( int ) statusCode;
            Url = url.AbsoluteUri;
            Content = content;
        }
    }
}