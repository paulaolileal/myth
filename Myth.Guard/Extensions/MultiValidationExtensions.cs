using Myth.Guard;
using Myth.Validation;

namespace Myth.Extensions;

/// <summary>
/// Extension methods for multi-validation scenarios providing fluent API for validating multiple values
/// </summary>
public static class MultiValidationExtensions {

	/// <summary>
	/// Extension method to easily add validation builders to Validate.All() fluent chain
	/// </summary>
	/// <param name="builder">The multi-validation builder</param>
	/// <param name="validationBuilderFactory">Factory function to create a validation builder</param>
	/// <returns>The multi-validation builder for method chaining</returns>
	public static IMultiValidationBuilder Add( this IMultiValidationBuilder builder, Func<IStandaloneValidationBuilder> validationBuilderFactory ) {
		if ( validationBuilderFactory != null ) {
			var validation = validationBuilderFactory( );
			builder.Add( validation );
		}
		return builder;
	}

	/// <summary>
	/// Extension method to validate a string value with configuration
	/// </summary>
	/// <param name="builder">The multi-validation builder</param>
	/// <param name="value">The string value to validate</param>
	/// <param name="propertyName">The property name for error context</param>
	/// <param name="configure">Configuration action for the validation builder</param>
	/// <returns>The multi-validation builder for method chaining</returns>
	public static IMultiValidationBuilder ValidateValue( this IMultiValidationBuilder builder, string? value, string propertyName, Func<IStandaloneValidationBuilder<string>, IStandaloneValidationBuilder<string>> configure ) {
		var validation = Guard.Guard.For( value, propertyName );
		var configuredValidation = configure( validation );
		return builder.Add( configuredValidation );
	}

	/// <summary>
	/// Extension method to validate an integer value with configuration
	/// </summary>
	/// <param name="builder">The multi-validation builder</param>
	/// <param name="value">The integer value to validate</param>
	/// <param name="propertyName">The property name for error context</param>
	/// <param name="configure">Configuration action for the validation builder</param>
	/// <returns>The multi-validation builder for method chaining</returns>
	public static IMultiValidationBuilder ValidateValue( this IMultiValidationBuilder builder, int value, string propertyName, Func<IStandaloneValidationBuilder<int>, IStandaloneValidationBuilder<int>> configure ) {
		var validation = Myth.Guard.Guard.For( value, propertyName );
		var configuredValidation = configure( validation );
		return builder.Add( configuredValidation );
	}

	/// <summary>
	/// Extension method to validate a string value with common string validations
	/// </summary>
	/// <param name="builder">The multi-validation builder</param>
	/// <param name="value">The string value to validate</param>
	/// <param name="propertyName">The property name for error context</param>
	/// <param name="configure">Configuration action for the string validation builder</param>
	/// <returns>The multi-validation builder for method chaining</returns>
	public static IMultiValidationBuilder ValidateString( this IMultiValidationBuilder builder, string? value, string propertyName, Func<IStandaloneValidationBuilder<string>, IStandaloneValidationBuilder<string>> configure ) {
		var validation = Myth.Guard.Guard.For( value, propertyName );
		var configuredValidation = configure( validation );
		return builder.Add( configuredValidation );
	}

	/// <summary>
	/// Extension method to validate an email address
	/// </summary>
	/// <param name="builder">The multi-validation builder</param>
	/// <param name="email">The email value to validate</param>
	/// <param name="propertyName">The property name for error context (defaults to "Email")</param>
	/// <param name="required">Whether the email is required (defaults to true)</param>
	/// <returns>The multi-validation builder for method chaining</returns>
	public static IMultiValidationBuilder ValidateEmail( this IMultiValidationBuilder builder, string? email, string propertyName = "Email", bool required = true ) {
		var validation = Myth.Guard.Guard.For( email, propertyName );

		if ( required ) {
			validation = validation.NotEmpty( );
		}

		validation = validation.Email( );

		return builder.Add( validation );
	}

	/// <summary>
	/// Extension method to validate a required string field (not null or empty)
	/// </summary>
	/// <param name="builder">The multi-validation builder</param>
	/// <param name="value">The string value to validate</param>
	/// <param name="propertyName">The property name for error context</param>
	/// <returns>The multi-validation builder for method chaining</returns>
	public static IMultiValidationBuilder ValidateRequiredString( this IMultiValidationBuilder builder, string? value, string propertyName ) {
		var validation = Myth.Guard.Guard.For( value, propertyName ).NotNull( ).NotEmpty( );
		return builder.Add( validation );
	}

	/// <summary>
	/// Extension method to validate a required field (not null)
	/// </summary>
	/// <param name="builder">The multi-validation builder</param>
	/// <param name="value">The value to validate</param>
	/// <param name="propertyName">The property name for error context</param>
	/// <returns>The multi-validation builder for method chaining</returns>
	public static IMultiValidationBuilder ValidateRequired( this IMultiValidationBuilder builder, string? value, string propertyName ) {
		var validation = Myth.Guard.Guard.For( value, propertyName ).NotNull( ).NotEmpty( );
		return builder.Add( validation );
	}


	/// <summary>
	/// Extension method to validate a numeric range
	/// </summary>
	/// <param name="builder">The multi-validation builder</param>
	/// <param name="value">The numeric value to validate</param>
	/// <param name="propertyName">The property name for error context</param>
	/// <param name="min">The minimum allowed value</param>
	/// <param name="max">The maximum allowed value</param>
	/// <returns>The multi-validation builder for method chaining</returns>
	public static IMultiValidationBuilder ValidateRange( this IMultiValidationBuilder builder, int value, string propertyName, int min, int max ) {
		var validation = Myth.Guard.Guard.For( value, propertyName ).Between( min, max );
		return builder.Add( validation );
	}

	/// <summary>
	/// Extension method to validate a numeric range for decimal values
	/// </summary>
	/// <param name="builder">The multi-validation builder</param>
	/// <param name="value">The decimal value to validate</param>
	/// <param name="propertyName">The property name for error context</param>
	/// <param name="min">The minimum allowed value</param>
	/// <param name="max">The maximum allowed value</param>
	/// <returns>The multi-validation builder for method chaining</returns>
	public static IMultiValidationBuilder ValidateRange( this IMultiValidationBuilder builder, decimal value, string propertyName, decimal min, decimal max ) {
		var validation = Myth.Guard.Guard.For( value, propertyName ).Between( min, max );
		return builder.Add( validation );
	}

	/// <summary>
	/// Validates multiple values using a simple syntax
	/// </summary>
	/// <param name="validations">Array of validation builders</param>
	/// <param name="serviceProvider">Optional service provider for async validation rules</param>
	/// <param name="cancellationToken">Cancellation token for async operations</param>
	/// <returns>Combined validation result</returns>
	public static Task<MultiValidationResult> ValidateAllAsync(
		this IStandaloneValidationBuilder[ ] validations,
		IServiceProvider? serviceProvider = null,
		CancellationToken cancellationToken = default ) {

		return Validate.AllAsync( validations, serviceProvider, cancellationToken );
	}

	/// <summary>
	/// Validates multiple values and throws if any validation fails
	/// </summary>
	/// <param name="validations">Array of validation builders</param>
	/// <param name="serviceProvider">Optional service provider for async validation rules</param>
	/// <param name="cancellationToken">Cancellation token for async operations</param>
	/// <exception cref="Myth.Exceptions.ValidationException">Thrown when any validation fails</exception>
	public static Task ValidateAllAndThrowAsync(
		this IStandaloneValidationBuilder[ ] validations,
		IServiceProvider? serviceProvider = null,
		CancellationToken cancellationToken = default ) {

		return Validate.AllAndThrowAsync( validations, serviceProvider, cancellationToken );
	}
}
