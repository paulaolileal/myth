using System.Net;

namespace Myth.Exceptions;

/// <summary>
/// Exception thrown when an error occurs during pipeline execution.
/// Used to signal failures in pipeline steps or logic.
/// </summary>
public class PipelineException : Exception {

	/// <summary>
	/// Gets the HTTP status code associated with this failure, if any.
	/// Automatically propagated from inner <see cref="PipelineException"/> when not explicitly set.
	/// </summary>
	public HttpStatusCode? StatusCode { get; }

	/// <summary>
	/// Initializes a new instance of <see cref="PipelineException"/> with a specified error message.
	/// </summary>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	public PipelineException( string message )
		: base( message ) { }

	/// <summary>
	/// Initializes a new instance of <see cref="PipelineException"/> with a specified error message
	/// and a reference to the inner exception. Automatically propagates <see cref="StatusCode"/>
	/// from the inner exception if it is also a <see cref="PipelineException"/>.
	/// </summary>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	/// <param name="innerException">The exception that is the cause of the current exception, or a null reference.</param>
	public PipelineException( string message, Exception? innerException )
		: base( message, innerException ) {
		StatusCode = ( innerException as PipelineException )?.StatusCode;
	}

	/// <summary>
	/// Initializes a new instance of <see cref="PipelineException"/> with a specified error message,
	/// inner exception, and an explicit HTTP status code.
	/// </summary>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	/// <param name="innerException">The exception that is the cause of the current exception, or a null reference.</param>
	/// <param name="statusCode">The HTTP status code to associate with this failure.</param>
	public PipelineException( string message, Exception? innerException, HttpStatusCode? statusCode )
		: base( message, innerException ) {
		StatusCode = statusCode ?? ( innerException as PipelineException )?.StatusCode;
	}
}
