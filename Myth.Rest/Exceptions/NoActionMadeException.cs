namespace Myth.Exceptions;

public class NoActionMadeException : Exception {

	public NoActionMadeException( )
		: base( $"No requests have been made! Use a `get`, `post`, `put`, `delete` before building." ) {
	}
}