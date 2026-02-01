using System.Net;
using Myth.Exceptions;
using Myth.Models;
using Myth.Validation;

namespace Myth.Guard;

/// <summary>
/// Static entry point for standalone validation operations outside of model context.
/// Provides a fluent API for validating individual values with comprehensive validation rules.
/// </summary>
public static class Sentry {

	/// <summary>
	/// Starts validation for a string value
	/// </summary>
	/// <param name="value">The string value to validate</param>
	/// <param name="propertyName">Optional property name for error context (defaults to "Value")</param>
	/// <returns>A validation builder for configuring string validation rules</returns>
	public static IStandaloneValidationBuilder<string> For( string? value, string propertyName = "Value" ) {
		return new StandaloneValidationBuilder<string>( value, propertyName );
	}

	/// <summary>
	/// Starts validation for an integer value
	/// </summary>
	/// <param name="value">The integer value to validate</param>
	/// <param name="propertyName">Optional property name for error context (defaults to "Value")</param>
	/// <returns>A validation builder for configuring integer validation rules</returns>
	public static IStandaloneValidationBuilder<int> For( int value, string propertyName = "Value" ) {
		return new StandaloneValidationBuilder<int>( value, propertyName );
	}

	/// <summary>
	/// Starts validation for a nullable integer value
	/// </summary>
	/// <param name="value">The nullable integer value to validate</param>
	/// <param name="propertyName">Optional property name for error context (defaults to "Value")</param>
	/// <returns>A validation builder for configuring nullable integer validation rules</returns>
	public static IStandaloneValidationBuilder<int?> For( int? value, string propertyName = "Value" ) {
		return new StandaloneValidationBuilder<int?>( value, propertyName );
	}

	/// <summary>
	/// Starts validation for a decimal value
	/// </summary>
	/// <param name="value">The decimal value to validate</param>
	/// <param name="propertyName">Optional property name for error context (defaults to "Value")</param>
	/// <returns>A validation builder for configuring decimal validation rules</returns>
	public static IStandaloneValidationBuilder<decimal> For( decimal value, string propertyName = "Value" ) {
		return new StandaloneValidationBuilder<decimal>( value, propertyName );
	}

	/// <summary>
	/// Starts validation for a nullable decimal value
	/// </summary>
	/// <param name="value">The nullable decimal value to validate</param>
	/// <param name="propertyName">Optional property name for error context (defaults to "Value")</param>
	/// <returns>A validation builder for configuring nullable decimal validation rules</returns>
	public static IStandaloneValidationBuilder<decimal?> For( decimal? value, string propertyName = "Value" ) {
		return new StandaloneValidationBuilder<decimal?>( value, propertyName );
	}

	/// <summary>
	/// Starts validation for a double value
	/// </summary>
	/// <param name="value">The double value to validate</param>
	/// <param name="propertyName">Optional property name for error context (defaults to "Value")</param>
	/// <returns>A validation builder for configuring double validation rules</returns>
	public static IStandaloneValidationBuilder<double> For( double value, string propertyName = "Value" ) {
		return new StandaloneValidationBuilder<double>( value, propertyName );
	}

	/// <summary>
	/// Starts validation for a nullable double value
	/// </summary>
	/// <param name="value">The nullable double value to validate</param>
	/// <param name="propertyName">Optional property name for error context (defaults to "Value")</param>
	/// <returns>A validation builder for configuring nullable double validation rules</returns>
	public static IStandaloneValidationBuilder<double?> For( double? value, string propertyName = "Value" ) {
		return new StandaloneValidationBuilder<double?>( value, propertyName );
	}

	/// <summary>
	/// Starts validation for a DateTime value
	/// </summary>
	/// <param name="value">The DateTime value to validate</param>
	/// <param name="propertyName">Optional property name for error context (defaults to "Value")</param>
	/// <returns>A validation builder for configuring DateTime validation rules</returns>
	public static IStandaloneValidationBuilder<DateTime> For( DateTime value, string propertyName = "Value" ) {
		return new StandaloneValidationBuilder<DateTime>( value, propertyName );
	}

	/// <summary>
	/// Starts validation for a nullable DateTime value
	/// </summary>
	/// <param name="value">The nullable DateTime value to validate</param>
	/// <param name="propertyName">Optional property name for error context (defaults to "Value")</param>
	/// <returns>A validation builder for configuring nullable DateTime validation rules</returns>
	public static IStandaloneValidationBuilder<DateTime?> For( DateTime? value, string propertyName = "Value" ) {
		return new StandaloneValidationBuilder<DateTime?>( value, propertyName );
	}

	/// <summary>
	/// Starts validation for a boolean value
	/// </summary>
	/// <param name="value">The boolean value to validate</param>
	/// <param name="propertyName">Optional property name for error context (defaults to "Value")</param>
	/// <returns>A validation builder for configuring boolean validation rules</returns>
	public static IStandaloneValidationBuilder<bool> For( bool value, string propertyName = "Value" ) {
		return new StandaloneValidationBuilder<bool>( value, propertyName );
	}

	/// <summary>
	/// Starts validation for a nullable boolean value
	/// </summary>
	/// <param name="value">The nullable boolean value to validate</param>
	/// <param name="propertyName">Optional property name for error context (defaults to "Value")</param>
	/// <returns>A validation builder for configuring nullable boolean validation rules</returns>
	public static IStandaloneValidationBuilder<bool?> For( bool? value, string propertyName = "Value" ) {
		return new StandaloneValidationBuilder<bool?>( value, propertyName );
	}

	/// <summary>
	/// Starts validation for an enum value
	/// </summary>
	/// <typeparam name="TEnum">The enum type</typeparam>
	/// <param name="value">The enum value to validate</param>
	/// <param name="propertyName">Optional property name for error context (defaults to "Value")</param>
	/// <returns>A validation builder for configuring enum validation rules</returns>
	public static IStandaloneValidationBuilder<TEnum> For<TEnum>( TEnum value, string propertyName = "Value" ) where TEnum : struct, Enum {
		return new StandaloneValidationBuilder<TEnum>( value, propertyName );
	}

	/// <summary>
	/// Starts validation for a nullable enum value
	/// </summary>
	/// <typeparam name="TEnum">The enum type</typeparam>
	/// <param name="value">The nullable enum value to validate</param>
	/// <param name="propertyName">Optional property name for error context (defaults to "Value")</param>
	/// <returns>A validation builder for configuring nullable enum validation rules</returns>
	public static IStandaloneValidationBuilder<TEnum?> For<TEnum>( TEnum? value, string propertyName = "Value" ) where TEnum : struct, Enum {
		return new StandaloneValidationBuilder<TEnum?>( value, propertyName );
	}

	/// <summary>
	/// Starts validation for a collection value
	/// </summary>
	/// <typeparam name="T">The collection element type</typeparam>
	/// <param name="value">The collection value to validate</param>
	/// <param name="propertyName">Optional property name for error context (defaults to "Value")</param>
	/// <returns>A validation builder for configuring collection validation rules</returns>
	public static IStandaloneValidationBuilder<IEnumerable<T>> For<T>( IEnumerable<T>? value, string propertyName = "Value" ) {
		return new StandaloneValidationBuilder<IEnumerable<T>>( value, propertyName );
	}

	/// <summary>
	/// Starts validation for a dictionary value
	/// </summary>
	/// <typeparam name="TKey">The dictionary key type</typeparam>
	/// <typeparam name="TValue">The dictionary value type</typeparam>
	/// <param name="value">The dictionary value to validate</param>
	/// <param name="propertyName">Optional property name for error context (defaults to "Value")</param>
	/// <returns>A validation builder for configuring dictionary validation rules</returns>
	public static IStandaloneValidationBuilder<IDictionary<TKey, TValue>> For<TKey, TValue>( IDictionary<TKey, TValue>? value, string propertyName = "Value" ) {
		return new StandaloneValidationBuilder<IDictionary<TKey, TValue>>( value, propertyName );
	}

	#region Manual Validation Failure

	/// <summary>
	/// Throws a validation exception with a single error message for the default "Value" field.
	/// Use this when you need to manually fail validation with a simple error message.
	/// </summary>
	/// <param name="message">The error message describing why validation failed</param>
	/// <param name="statusCode">Optional HTTP status code (defaults to BadRequest/400)</param>
	/// <exception cref="ValidationException">Always throws with the specified error</exception>
	/// <example>
	/// <code>
	/// if (complexCondition)
	/// {
	///     Sentry.Fail("Complex validation failed");
	/// }
	/// </code>
	/// </example>
	public static void Fail( string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest ) {
		Fail( "Value", message, statusCode );
	}

	/// <summary>
	/// Throws a validation exception with a single error for a specific field.
	/// Use this when you need to manually fail validation for a particular field.
	/// </summary>
	/// <param name="field">The name of the field that failed validation</param>
	/// <param name="message">The error message describing why validation failed</param>
	/// <param name="statusCode">Optional HTTP status code (defaults to BadRequest/400)</param>
	/// <exception cref="ValidationException">Always throws with the specified error</exception>
	/// <example>
	/// <code>
	/// if (user.Age &lt; 18 &amp;&amp; user.RequiresParentalConsent)
	/// {
	///     Sentry.Fail("Age", "User must be 18 or older or have parental consent");
	/// }
	/// </code>
	/// </example>
	public static void Fail( string field, string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest ) {
		var error = new ValidationError( field, message, statusCode );
		Fail( error );
	}

	/// <summary>
	/// Throws a validation exception with a single validation error.
	/// Use this when you need full control over the validation error details.
	/// </summary>
	/// <param name="error">The validation error to throw</param>
	/// <exception cref="ValidationException">Always throws with the specified error</exception>
	/// <exception cref="ArgumentNullException">Thrown when error is null</exception>
	/// <example>
	/// <code>
	/// var error = new ValidationError(
	///     "Email",
	///     "Email already exists in the system",
	///     HttpStatusCode.Conflict,
	///     new[] { "user@example.com" }
	/// );
	/// Sentry.Fail(error);
	/// </code>
	/// </example>
	public static void Fail( ValidationError error ) {
		ArgumentNullException.ThrowIfNull( error, nameof( error ) );
		Fail( new[ ] { error } );
	}

	/// <summary>
	/// Throws a validation exception with multiple validation errors.
	/// Use this when you need to fail validation with several errors at once.
	/// </summary>
	/// <param name="errors">The collection of validation errors to throw</param>
	/// <exception cref="ValidationException">Always throws with the specified errors</exception>
	/// <exception cref="ArgumentNullException">Thrown when errors is null</exception>
	/// <exception cref="ArgumentException">Thrown when errors collection is empty</exception>
	/// <example>
	/// <code>
	/// var errors = new List&lt;ValidationError&gt;
	/// {
	///     new ValidationError("Name", "Name is required", HttpStatusCode.BadRequest),
	///     new ValidationError("Email", "Email is required", HttpStatusCode.BadRequest)
	/// };
	/// Sentry.Fail(errors);
	/// </code>
	/// </example>
	public static void Fail( IEnumerable<ValidationError> errors ) {
		ArgumentNullException.ThrowIfNull( errors, nameof( errors ) );

		var errorList = errors.ToList( );

		if ( errorList.Count == 0 ) {
			throw new ArgumentException( "At least one validation error must be provided", nameof( errors ) );
		}

		var validationResult = new ValidationResult( errorList );
		throw new ValidationException( validationResult );
	}

	#endregion

}
