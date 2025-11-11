using System.Net;

namespace Myth.Exceptions;

public class NotMappedResultTypeException( HttpStatusCode statusCode, string content ) : Exception( $"No types have been mapped to `{( int )statusCode} {statusCode}` status code!"
	+ $"\nContent to be mapped:"
	+ $"\n{content}" ) {
}
