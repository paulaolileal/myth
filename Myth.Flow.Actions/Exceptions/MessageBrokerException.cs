/// <summary>
/// Thrown when message broker operations fail
/// </summary>
public sealed class MessageBrokerException : Exception {

	/// <summary>
	/// Initializes a new instance of the MessageBrokerException class with a specified error message
	/// </summary>
	/// <param name="message">The message that describes the error</param>
	public MessageBrokerException( string message ) : base( message ) {
	}

	/// <summary>
	/// Initializes a new instance of the MessageBrokerException class with a specified error message and inner exception
	/// </summary>
	/// <param name="message">The message that describes the error</param>
	/// <param name="innerException">The exception that is the cause of the current exception</param>
	public MessageBrokerException( string message, Exception innerException )
		: base( message, innerException ) { }
}