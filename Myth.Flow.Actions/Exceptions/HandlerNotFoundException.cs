/// <summary>
/// Thrown when no handler is found for a request
/// </summary>
public sealed class HandlerNotFoundException : InvalidOperationException {

	/// <summary>
	/// Initializes a new instance of the HandlerNotFoundException class with a specified error message
	/// </summary>
	/// <param name="message">The message that describes the error</param>
	public HandlerNotFoundException( string message ) : base( message ) {
	}

	/// <summary>
	/// Initializes a new instance of the HandlerNotFoundException class with a specified error message and inner exception
	/// </summary>
	/// <param name="message">The message that describes the error</param>
	/// <param name="innerException">The exception that is the cause of the current exception</param>
	public HandlerNotFoundException( string message, Exception innerException )
		: base( message, innerException ) { }
}
