namespace Myth.Exceptions;

public class SpecificationException( string message, Exception? exception ) : Exception( message, exception ) {
}