using System.Net;

namespace Myth.Exceptions;

public class ParsingTypeException( HttpStatusCode statusCode, Type typeInformed, string contentReceived, Exception innerException ) : Exception( $"The type informed on this response status code couldn't be use for parsing the content" +
				$"\nStatusCode: `{statusCode}`" +
				$"\nTypeInformed: `{typeInformed}`" +
				$"\nContentReceived: `{contentReceived}`", innerException ) {
	public HttpStatusCode StatusCode { get; private set; } = statusCode;
	public Type TypeInformed { get; private set; } = typeInformed;
	public string ContentReceived { get; private set; } = contentReceived;
}
