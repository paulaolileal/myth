using System.Net;

namespace Myth.Models;

/// <summary>
/// Represents the result of a validation operation, containing any validation errors and status information.
/// This class provides a comprehensive view of validation outcomes, including error details and HTTP status codes.
/// </summary>
public sealed class ValidationResult {
	private readonly List<ValidationError> _errors = new( );

	/// <summary>
	/// Gets a read-only collection of validation errors that occurred during validation.
	/// </summary>
	/// <value>
	/// A collection of <see cref="ValidationError"/> objects describing each validation failure.
	/// Empty if validation was successful.
	/// </value>
	public IReadOnlyList<ValidationError> Errors => _errors;

	/// <summary>
	/// Gets a value indicating whether the validation was successful (no errors occurred).
	/// </summary>
	/// <value>
	/// <c>true</c> if no validation errors occurred; otherwise, <c>false</c>.
	/// </value>
	public bool IsValid => _errors.Count == 0;

	/// <summary>
	/// Gets the HTTP status code that should be returned based on the validation result.
	/// If there are validation errors, returns the highest status code among all errors.
	/// If validation was successful, returns <see cref="HttpStatusCode.OK"/>.
	/// </summary>
	/// <value>
	/// The appropriate HTTP status code for the validation result.
	/// Common values include <see cref="HttpStatusCode.BadRequest"/> for general validation errors,
	/// <see cref="HttpStatusCode.Conflict"/> for duplicate entries, and <see cref="HttpStatusCode.NotFound"/> for missing references.
	/// </value>
	public HttpStatusCode StatusCode => Errors.Any( )
		? Errors.Max( e => e.StatusCode )
		: HttpStatusCode.OK;

	/// <summary>
	/// Initializes a new instance of the <see cref="ValidationResult"/> class with no errors.
	/// </summary>
	public ValidationResult( ) {
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ValidationResult"/> class with the specified errors.
	/// </summary>
	/// <param name="errors">The validation errors to include in the result.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="errors"/> is null.</exception>
	public ValidationResult( List<ValidationError> errors ) {
		ArgumentNullException.ThrowIfNull( errors, nameof( errors ) );
		_errors = errors;
	}

	/// <summary>
	/// Adds a validation error to the result.
	/// </summary>
	/// <param name="error">The validation error to add.</param>
	/// <remarks>
	/// This method is used internally by the validation framework to build the result.
	/// </remarks>
	internal void AddError( ValidationError error ) {
		_errors.Add( error );
	}

	/// <summary>
	/// Adds multiple validation errors to the result.
	/// </summary>
	/// <param name="errors">The validation errors to add.</param>
	/// <remarks>
	/// This method is used internally by the validation framework to build the result.
	/// </remarks>
	internal void AddErrors( IEnumerable<ValidationError> errors ) {
		_errors.AddRange( errors );
	}
}
