using Myth.Models;

namespace Myth.Validation;

/// <summary>
/// Represents the result of a standalone validation operation with success status and error details
/// </summary>
/// <remarks>
/// Initializes a new instance of StandaloneValidationResult
/// </remarks>
/// <param name="errors">The validation errors found during validation</param>
public class StandaloneValidationResult( IEnumerable<ValidationError> errors ) {

	/// <summary>
	/// Gets whether the validation was successful (no errors found)
	/// </summary>
	public bool IsValid => !Errors.Any( );

	/// <summary>
	/// Gets whether the validation failed (errors found)
	/// </summary>
	public bool IsInvalid => !IsValid;

	/// <summary>
	/// Gets the collection of validation errors found during validation
	/// </summary>
	public IReadOnlyList<ValidationError> Errors { get; } = errors?.ToList( ).AsReadOnly( ) ?? new List<ValidationError>( ).AsReadOnly( );

	/// <summary>
	/// Gets the first validation error if any exist, otherwise null
	/// </summary>
	public ValidationError? FirstError => Errors.FirstOrDefault( );

	/// <summary>
	/// Gets all error messages concatenated with newline separators
	/// </summary>
	public string ErrorMessage => string.Join( Environment.NewLine, Errors.Select( e => e.Message ) );

	/// <summary>
	/// Creates a successful validation result with no errors
	/// </summary>
	/// <returns>A successful validation result</returns>
	public static StandaloneValidationResult Success( ) {
		return new StandaloneValidationResult( Enumerable.Empty<ValidationError>( ) );
	}

	/// <summary>
	/// Creates a failed validation result with a single error
	/// </summary>
	/// <param name="error">The validation error</param>
	/// <returns>A failed validation result</returns>
	public static StandaloneValidationResult Failure( ValidationError error ) {
		return new StandaloneValidationResult( [ error ] );
	}

	/// <summary>
	/// Creates a failed validation result with multiple errors
	/// </summary>
	/// <param name="errors">The validation errors</param>
	/// <returns>A failed validation result</returns>
	public static StandaloneValidationResult Failure( IEnumerable<ValidationError> errors ) {
		return new StandaloneValidationResult( errors );
	}

	/// <summary>
	/// Creates a failed validation result with a simple error message
	/// </summary>
	/// <param name="propertyName">The property name that failed validation</param>
	/// <param name="message">The error message</param>
	/// <param name="statusCode">Optional HTTP status code</param>
	/// <param name="options">Optional list of valid options</param>
	/// <returns>A failed validation result</returns>
	public static StandaloneValidationResult Failure( string propertyName, string message, int? statusCode = null, IReadOnlyList<string>? options = null ) {
		var httpStatusCode = statusCode.HasValue ? ( System.Net.HttpStatusCode )statusCode.Value : System.Net.HttpStatusCode.BadRequest;
		var error = new ValidationError {
			Field = propertyName,
			Message = message,
			StatusCode = httpStatusCode,
			Options = options
		};
		return new StandaloneValidationResult( [ error ] );
	}
}
