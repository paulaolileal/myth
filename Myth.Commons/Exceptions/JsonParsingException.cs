namespace Myth.Exceptions;

public class JsonParsingException( string? message, Exception? innerException )
	: Exception( message, innerException ) {
}
