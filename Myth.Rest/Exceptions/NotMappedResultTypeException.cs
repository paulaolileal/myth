using System.Net;

namespace Myth.Exceptions;

public class NotMappedResultTypeException( HttpStatusCode statusCode ) : Exception( $"No types have been mapped to `{( int )statusCode} {statusCode}` status code!" ) {
}