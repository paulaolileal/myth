using System.Net;

namespace Myth.Exceptions {

	public class ParsingTypeException : Exception {
		public HttpStatusCode StatusCode { get; private set; }
		public Type TypeInformed { get; private set; }
		public string ContentReceived { get; private set; }

		public ParsingTypeException( HttpStatusCode statusCode, Type typeInformed, string contentReceived, Exception innerException )
			: base( $"The type informed on this response status code couldn't be use for parsing the content" +
					$"\nStatusCode: `{statusCode}`" +
					$"\nTypeInformed: `{typeInformed}`" +
					$"\nContentReceived: `{contentReceived}`", innerException ) {
			StatusCode = statusCode;
			TypeInformed = typeInformed;
			ContentReceived = contentReceived;
		}
	}
}