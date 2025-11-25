using Myth.Validation;

namespace Myth.Guard;

/// <summary>
/// Static entry point for standalone validation operations outside of model context.
/// Provides a fluent API for validating individual values with comprehensive validation rules.
/// </summary>
public static class Guard {

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

}
