namespace Myth.Exceptions;

/// <summary>
/// Exception thrown when no mapping or binder can be found for the specified source and destination types.
/// This typically occurs when attempting to transform an object but no registered mapping exists
/// between the source type and the target destination type.
/// </summary>
/// <remarks>
/// This exception is part of the Myth Morph mapping system and indicates a configuration issue
/// where the required type mappings have not been properly registered in the SchemaRegistry.
/// </remarks>
public class BinderNotFoundException : Exception {

	/// <summary>
	/// Initializes a new instance of the BinderNotFoundException class.
	/// </summary>
	public BinderNotFoundException( ) {
	}

	/// <summary>
	/// Initializes a new instance of the BinderNotFoundException class with a specified error message.
	/// </summary>
	/// <param name="message">The message that describes the error, typically indicating which types lack a mapping.</param>
	public BinderNotFoundException( string? message ) : base( message ) {
	}

	/// <summary>
	/// Initializes a new instance of the BinderNotFoundException class with a specified error message
	/// and a reference to the inner exception that is the cause of this exception.
	/// </summary>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	/// <param name="innerException">The exception that is the cause of the current exception.</param>
	public BinderNotFoundException( string? message, Exception? innerException ) : base( message, innerException ) {
	}
}
