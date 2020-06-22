using Myth.ViewModels.Errors;
using System;
using System.Collections.Generic;

namespace Myth.Exceptions {

    public class ValidationException: Exception {
        public IEnumerable<MessageResponse> Errors { get; }

        protected ValidationException( ) {
        }

        public ValidationException( IEnumerable<MessageResponse> errors, string message )
            : base( message ) {
            Errors = errors;
        }

        public ValidationException( IEnumerable<MessageResponse> errors, string message, Exception inner )
            : base( message, inner ) {
            Errors = errors;
        }
    }
}