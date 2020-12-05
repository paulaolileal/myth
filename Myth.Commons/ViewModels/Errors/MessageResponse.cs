namespace Myth.ViewModels.Errors {

    public class MessageResponse {
        public string PropertyName { get; private set; }

        public string ErrorMessage { get; private set; }

        public MessageResponse( string propertyName, string errorMessage ) {
            PropertyName = propertyName;
            ErrorMessage = errorMessage;
        }
    }
}