using Myth.Models;

namespace Myth.Validation;

/// <summary>
/// Represents the result of multiple standalone validation operations with aggregated success status and error details
/// </summary>
/// <remarks>
/// Initializes a new instance of MultiValidationResult
/// </remarks>
/// <param name="errors">The aggregated validation errors found during all validations</param>
public class MultiValidationResult( IEnumerable<ValidationError> errors ) {

	/// <summary>
	/// Gets whether all validations were successful (no errors found)
	/// </summary>
	public bool IsValid => !Errors.Any( );

	/// <summary>
	/// Gets whether any validation failed (errors found)
	/// </summary>
	public bool IsInvalid => !IsValid;

	/// <summary>
	/// Gets the collection of all validation errors found during all validations
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
	/// Gets errors grouped by field name for easier processing
	/// </summary>
	public IReadOnlyDictionary<string, IReadOnlyList<ValidationError>> ErrorsByField =>
		Errors.GroupBy( e => e.Field )
			.ToDictionary( g => g.Key, g => ( IReadOnlyList<ValidationError> )g.ToList( ).AsReadOnly( ) );

	/// <summary>
	/// Gets the count of validation errors
	/// </summary>
	public int ErrorCount => Errors.Count;

	/// <summary>
	/// Gets the count of unique fields that have validation errors
	/// </summary>
	public int FieldsWithErrorsCount => ErrorsByField.Count;

	/// <summary>
	/// Creates a successful validation result with no errors
	/// </summary>
	/// <returns>A successful validation result</returns>
	public static MultiValidationResult Success( ) {
		return new MultiValidationResult( Enumerable.Empty<ValidationError>( ) );
	}

	/// <summary>
	/// Creates a failed validation result with a single error
	/// </summary>
	/// <param name="error">The validation error</param>
	/// <returns>A failed validation result</returns>
	public static MultiValidationResult Failure( ValidationError error ) {
		return new MultiValidationResult( [ error ] );
	}

	/// <summary>
	/// Creates a failed validation result with multiple errors
	/// </summary>
	/// <param name="errors">The validation errors</param>
	/// <returns>A failed validation result</returns>
	public static MultiValidationResult Failure( IEnumerable<ValidationError> errors ) {
		return new MultiValidationResult( errors );
	}

	/// <summary>
	/// Creates a failed validation result with a simple error message
	/// </summary>
	/// <param name="propertyName">The property name that failed validation</param>
	/// <param name="message">The error message</param>
	/// <param name="statusCode">Optional HTTP status code</param>
	/// <param name="options">Optional list of valid options</param>
	/// <returns>A failed validation result</returns>
	public static MultiValidationResult Failure( string propertyName, string message, int? statusCode = null, IReadOnlyList<string>? options = null ) {
		var httpStatusCode = statusCode.HasValue ? ( System.Net.HttpStatusCode )statusCode.Value : System.Net.HttpStatusCode.BadRequest;
		var error = new ValidationError {
			Field = propertyName,
			Message = message,
			StatusCode = httpStatusCode,
			Options = options
		};
		return new MultiValidationResult( [ error ] );
	}

	/// <summary>
	/// Checks if a specific field has validation errors
	/// </summary>
	/// <param name="fieldName">The field name to check</param>
	/// <returns>True if the field has errors, false otherwise</returns>
	public bool HasErrorsForField( string fieldName ) {
		return ErrorsByField.ContainsKey( fieldName );
	}

	/// <summary>
	/// Gets validation errors for a specific field
	/// </summary>
	/// <param name="fieldName">The field name to get errors for</param>
	/// <returns>Collection of validation errors for the field, or empty collection if no errors</returns>
	public IReadOnlyList<ValidationError> GetErrorsForField( string fieldName ) {
		return ErrorsByField.TryGetValue( fieldName, out var errors )
			? errors
			: new List<ValidationError>( ).AsReadOnly( );
	}
}
