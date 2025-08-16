namespace Myth.Exceptions {

	/// Exception thrown when there is an error in property or field binding operations during object mapping.
	/// This can occur during expression parsing, value assignment, or when binding configurations are invalid.
	/// </summary>
	/// <remarks>
	/// This exception is typically thrown during the Schema configuration phase when invalid expressions
	/// are provided for property binding, or during the actual binding process when value assignment fails.
	/// </remarks>
	public class BindException : Exception {

		/// <summary>
		/// Initializes a new instance of the BindException class.
		/// </summary>
		public BindException( ) {
		}

		/// <summary>
		/// Initializes a new instance of the BindException class with a specified error message.
		/// </summary>
		/// <param name="message">The message that describes the binding error, such as invalid expressions or assignment failures.</param>
		public BindException( string? message ) : base( message ) {
		}

		/// <summary>
		/// Initializes a new instance of the BindException class with a specified error message
		/// and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception.</param>
		public BindException( string? message, Exception? innerException ) : base( message, innerException ) {
		}
	}
}