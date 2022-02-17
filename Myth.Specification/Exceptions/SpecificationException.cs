namespace Myth.Exceptions {

    public class SpecificationException : Exception {

        public SpecificationException( string message, Exception? exception ) : base( message, exception ) {
        }
    }
}