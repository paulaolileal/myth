namespace Myth.Exceptions;

public class NoAvailableTransactionException : Exception {

	public NoAvailableTransactionException( )
		: base( "No available transaction in the context! The action be executed!" ) {
	}
}