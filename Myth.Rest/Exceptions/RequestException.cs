namespace Myth.Exceptions {

    public class RequestException : Exception {

        public RequestException( ) : base( $"No requests have been made! Use a `get`, `post`, `put`, `delete` before building." ) {
        }
    }
}