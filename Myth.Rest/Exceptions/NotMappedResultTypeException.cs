using System.Net;

namespace Myth.Exceptions {

	public class NotMappedResultTypeException : Exception {

		public NotMappedResultTypeException( HttpStatusCode statusCode )
			: base( $"No types have been mapped to `{statusCode}` status code!" ) {
		}
	}
}