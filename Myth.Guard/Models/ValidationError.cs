using System.Net;

namespace Myth.Models; 

/// <summary>
/// Represents a single validation error that occurred during field validation.
/// Contains detailed information about the validation failure, including the field name,
/// error message, error code, and appropriate HTTP status code.
/// </summary>
public sealed class ValidationError {

	/// <summary>
	/// Gets or sets the name of the field that failed validation.
	/// </summary>
	/// <value>
	/// The field name as extracted from the validation expression, typically the property name.
	/// For example, if validating the "Email" property, this would be "Email".
	/// </value>
	public string Field { get; init; } = string.Empty;

	/// <summary>
	/// Gets or sets the human-readable error message describing the validation failure.
	/// </summary>
	/// <value>
	/// A descriptive message explaining why the validation failed.
	/// This message is intended for display to end users and should be clear and actionable.
	/// For example: "Email address is required" or "Password must be at least 8 characters long".
	/// </value>
	public string Message { get; init; } = string.Empty;

	/// <summary>
	/// Gets or sets the error code that categorizes the type of validation failure.
	/// </summary>
	/// <value>
	/// A string code that identifies the type of validation error.
	/// Common codes include "VIOLATION" for general validation failures,
	/// "EMAIL_EXISTS" for duplicate email addresses, or custom codes defined by validation rules.
	/// Default value is "VIOLATION".
	/// </value>
	public string Code { get; init; } = "VIOLATION";

	/// <summary>
	/// Gets or sets the HTTP status code that should be returned for this validation error.
	/// </summary>
	/// <value>
	/// The appropriate HTTP status code for this error type.
	/// Common values include <see cref="HttpStatusCode.BadRequest"/> for general validation errors,
	/// <see cref="HttpStatusCode.Conflict"/> for duplicate entries, and <see cref="HttpStatusCode.NotFound"/> for missing references.
	/// Default value is <see cref="HttpStatusCode.BadRequest"/>.
	/// </value>
	public HttpStatusCode StatusCode { get; init; } = HttpStatusCode.BadRequest;

	/// <summary>
	/// Initializes a new instance of the <see cref="ValidationError"/> class with default values.
	/// </summary>
	public ValidationError( ) {
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ValidationError"/> class with the specified error details.
	/// </summary>
	/// <param name="field">The name of the field that failed validation.</param>
	/// <param name="message">The human-readable error message.</param>
	/// <param name="code">The error code that categorizes the validation failure.</param>
	/// <param name="statusCode">The HTTP status code for this error.</param>
	public ValidationError( string field, string message, string code, HttpStatusCode statusCode ) {
		Field = field;
		Message = message;
		Code = code;
		StatusCode = statusCode;
	}

	/// <summary>
	/// Returns a string representation of the validation error in a structured format.
	/// </summary>
	/// <returns>
	/// A string in the format "Error on field `{Field}` with `{Code}: {Message}`
	/// that provides a concise summary of the validation error.
	/// </returns>
	/// <example>
	/// For a validation error with Field="Email", Code="INVALID_FORMAT", and Message="Email address is invalid",
	/// this method returns: "Error on field `Email` with `INVALID_FORMAT: Email address is invalid`"
	/// </example>
	public override string ToString( ) => $"Error on field `{Field}` with `{Code}: {Message}";
}
