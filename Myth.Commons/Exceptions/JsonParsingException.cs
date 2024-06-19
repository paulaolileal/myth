namespace Myth.Exceptions {

	public class JsonParsingException : Exception {

		public JsonParsingException( string? message, Exception? innerException ) : base( message, innerException ) {
		}
	}
}