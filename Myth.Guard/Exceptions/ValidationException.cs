using Myth.Models;

namespace Myth.Exceptions; 

/// <summary>
/// Exception thrown when validation fails during the validation process.
/// This exception is used to propagate validation failures with detailed error information,
/// allowing the application to handle validation errors appropriately.
/// </summary>
/// <remarks>
/// This exception is typically thrown by the <see cref="IValidator.ValidateAsync{T}(T, ValidationContextKey?, CancellationToken)"/> method
/// when validation rules fail. The exception contains a <see cref="ValidationResult"/> that provides
/// detailed information about all validation errors that occurred.
///
/// <para>
/// When using the ASP.NET Core middleware (<see cref="Middlewares.ValidationExceptionMiddleware"/>),
/// this exception is automatically caught and converted to an appropriate HTTP response
/// with structured error information.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// try
/// {
///     await validator.ValidateAsync(userDto, ValidationContextKey.Create);
/// }
/// catch (ValidationException ex)
/// {
///     foreach (var error in ex.ValidationResult.Errors)
///     {
///         Console.WriteLine($"{error.Field}: {error.Message}");
///     }
/// }
/// </code>
/// </example>
public sealed class ValidationException : Exception {

	/// <summary>
	/// Gets the validation result containing detailed information about all validation errors.
	/// </summary>
	/// <value>
	/// A <see cref="ValidationResult"/> instance that contains all validation errors,
	/// including field names, error messages, error codes, and HTTP status codes.
	/// </value>
	public ValidationResult ValidationResult { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ValidationException"/> class with the specified validation result.
	/// </summary>
	/// <param name="result">
	/// The validation result containing the validation errors.
	/// Must not be null and should contain at least one validation error.
	/// </param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
	/// <example>
	/// <code>
	/// var result = new ValidationResult();
	/// result.AddError(new ValidationError("Email", "Email is required", "REQUIRED", HttpStatusCode.BadRequest));
	/// throw new ValidationException(result);
	/// </code>
	/// </example>
	public ValidationException( ValidationResult result )
		: base( "Validation failed" + ( result is not null && !result.IsValid ? $" {result.Errors.Count} error(s)" : string.Empty ) ) {
		ArgumentNullException.ThrowIfNull( result, nameof( ValidationResult ) );
		ValidationResult = result;
	}
}
