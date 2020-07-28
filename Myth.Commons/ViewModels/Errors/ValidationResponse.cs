using System.Collections.Generic;

namespace Myth.ViewModels.Errors {

    public class ValidationResponse {
        public IEnumerable<MessageResponse> Errors { get; set; }

        public ValidationResponse( ) {
            Errors = new List<MessageResponse>( );
        }

        public ValidationResponse( IEnumerable<MessageResponse> errors ) {
            Errors = errors;
        }
    }
}