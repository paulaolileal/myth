using Myth.ViewModels.Errors;
using System;
using System.Collections.Generic;
using System.Net;

namespace Myth.Exceptions {

    public class ValidationException: Exception {
        public IEnumerable<MessageResponse> Errors { get; }

        public HttpStatusCode StatusCode { get; }

        protected ValidationException( ) {
        }

        public ValidationException( string message, HttpStatusCode statusCode, params MessageResponse[ ] messages )
            : base( message ) {
            StatusCode = statusCode;
            Errors = new List<MessageResponse>( messages );
        }

        public ValidationException( string message, params MessageResponse[ ] messages )
            : this( message, HttpStatusCode.UnprocessableEntity, messages ) {
        }

        public ValidationException( IEnumerable<MessageResponse> errors, string message, HttpStatusCode statusCode = HttpStatusCode.UnprocessableEntity )
            : base( message ) {
            Errors = errors;
            StatusCode = statusCode;
        }

        public ValidationException( IEnumerable<MessageResponse> errors, string message, Exception inner )
            : base( message, inner ) {
            Errors = errors;
            StatusCode = HttpStatusCode.UnprocessableEntity;
        }
    }
}