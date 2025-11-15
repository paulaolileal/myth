namespace Myth.Exceptions;

/// <summary>
/// Exception thrown when the Myth Morph system is not properly configured or required dependencies are missing.
/// This typically occurs when the dependency injection container is not set up correctly or when
/// required services like SchemaRegistry are not registered.
/// </summary>
/// <remarks>
/// This exception indicates a setup issue with the Morph system, usually meaning that
/// ServiceCollectionExtensions.AddMorph() was not called during application configuration,
/// or the service provider is not properly configured.
/// </remarks>
public class InvalidMorphConfigurationException : Exception {

	/// <summary>
	/// Initializes a new instance of the InvalidMorphConfigurationException class.
	/// </summary>
	public InvalidMorphConfigurationException( ) {
	}

	/// <summary>
	/// Initializes a new instance of the InvalidMorphConfigurationException class with a specified error message.
	/// </summary>
	/// <param name="message">The message that describes the configuration error, typically indicating what is missing or misconfigured.</param>
	public InvalidMorphConfigurationException( string? message ) : base( message ) {
	}

	/// <summary>
	/// Initializes a new instance of the InvalidMorphConfigurationException class with a specified error message
	/// and a reference to the inner exception that is the cause of this exception.
	/// </summary>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	/// <param name="innerException">The exception that is the cause of the current exception.</param>
	public InvalidMorphConfigurationException( string? message, Exception? innerException ) : base( message, innerException ) {
	}
}
