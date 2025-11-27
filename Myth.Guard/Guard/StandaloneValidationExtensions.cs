using Myth.Rules.Collections;
using Myth.Rules.Constants;
using Myth.Rules.DateTimes;
using Myth.Rules.Enums;
using Myth.Rules.Numerics;
using Myth.Rules.Strings;
using Myth.Validation;
using Myth.ValueObjects;

namespace Myth.Guard;

/// <summary>
/// Extension methods that provide type-specific validation rules for standalone validation builders.
/// These methods leverage the existing validation rule infrastructure to provide comprehensive validation capabilities.
/// </summary>
public static class StandaloneValidationExtensions {

	#region String Validation Extensions

	/// <summary>
	/// Adds a validation rule that the string must not be null or empty
	/// </summary>
	/// <param name="builder">The validation builder</param>
	/// <returns>The validation builder for method chaining</returns>
	public static IStandaloneValidationBuilder<string> NotEmpty( this IStandaloneValidationBuilder<string> builder ) {
		return builder.AddRule( new NotEmptyStringRule( ) );
	}

	/// <summary>
	/// Adds a validation rule that the string must have a minimum length
	/// </summary>
	/// <param name="builder">The validation builder</param>
	/// <param name="length">The minimum required length</param>
	/// <returns>The validation builder for method chaining</returns>
	public static IStandaloneValidationBuilder<string> MinimumLength( this IStandaloneValidationBuilder<string> builder, int length ) {
		return builder.AddRule( new MinimumLengthRule( length ) );
	}

	/// <summary>
	/// Adds a validation rule that the string must have a maximum length
	/// </summary>
	/// <param name="builder">The validation builder</param>
	/// <param name="length">The maximum allowed length</param>
	/// <returns>The validation builder for method chaining</returns>
	public static IStandaloneValidationBuilder<string> MaximumLength( this IStandaloneValidationBuilder<string> builder, int length ) {
		return builder.AddRule( new MaximumLengthRule( length ) );
	}

	/// <summary>
	/// Adds a validation rule that the string must be a valid email address
	/// </summary>
	/// <param name="builder">The validation builder</param>
	/// <returns>The validation builder for method chaining</returns>
	public static IStandaloneValidationBuilder<string> Email( this IStandaloneValidationBuilder<string> builder ) {
		return builder.AddRule( new EmailRule( ) );
	}

	/// <summary>
	/// Adds a validation rule that the string must match a regular expression pattern
	/// </summary>
	/// <param name="builder">The validation builder</param>
	/// <param name="pattern">The regex pattern to match</param>
	/// <returns>The validation builder for method chaining</returns>
	public static IStandaloneValidationBuilder<string> Matches( this IStandaloneValidationBuilder<string> builder, string pattern ) {
		return builder.AddRule( new MatchesRule( new System.Text.RegularExpressions.Regex( pattern ) ) );
	}

	/// <summary>
	/// Adds a validation rule that the string length must be between the specified bounds
	/// </summary>
	/// <param name="builder">The validation builder</param>
	/// <param name="min">The minimum length (inclusive)</param>
	/// <param name="max">The maximum length (inclusive)</param>
	/// <returns>The validation builder for method chaining</returns>
	public static IStandaloneValidationBuilder<string> LengthBetween( this IStandaloneValidationBuilder<string> builder, int min, int max ) {
		return builder.AddRule( new LengthBetweenRule( min, max ) );
	}

	#endregion

	#region Numeric Validation Extensions

	/// <summary>
	/// Adds a validation rule that the value must be greater than the specified value
	/// </summary>
	/// <param name="builder">The validation builder</param>
	/// <param name="value">The value to compare against</param>
	/// <returns>The validation builder for method chaining</returns>
	public static IStandaloneValidationBuilder<T> GreaterThan<T>( this IStandaloneValidationBuilder<T> builder, T value ) where T : struct, IComparable<T> {
		return builder.AddRule( new GreaterThanRule<T>( value ) );
	}

	/// <summary>
	/// Adds a validation rule that the value must be greater than or equal to the specified value
	/// </summary>
	/// <param name="builder">The validation builder</param>
	/// <param name="value">The value to compare against</param>
	/// <returns>The validation builder for method chaining</returns>
	public static IStandaloneValidationBuilder<T> GreaterThanOrEqualTo<T>( this IStandaloneValidationBuilder<T> builder, T value ) where T : struct, IComparable<T> {
		return builder.AddRule( new GreaterOrEqualsRule<T>( value ) );
	}

	/// <summary>
	/// Adds a validation rule that the value must be less than the specified value
	/// </summary>
	/// <param name="builder">The validation builder</param>
	/// <param name="value">The value to compare against</param>
	/// <returns>The validation builder for method chaining</returns>
	public static IStandaloneValidationBuilder<T> LessThan<T>( this IStandaloneValidationBuilder<T> builder, T value ) where T : struct, IComparable<T> {
		return builder.AddRule( new LessThanRule<T>( value ) );
	}

	/// <summary>
	/// Adds a validation rule that the value must be less than or equal to the specified value
	/// </summary>
	/// <param name="builder">The validation builder</param>
	/// <param name="value">The value to compare against</param>
	/// <returns>The validation builder for method chaining</returns>
	public static IStandaloneValidationBuilder<T> LessThanOrEqualTo<T>( this IStandaloneValidationBuilder<T> builder, T value ) where T : struct, IComparable<T> {
		return builder.AddRule( new LessOrEqualsRule<T>( value ) );
	}

	/// <summary>
	/// Adds a validation rule that the value must be between the specified bounds
	/// </summary>
	/// <param name="builder">The validation builder</param>
	/// <param name="min">The minimum value (inclusive)</param>
	/// <param name="max">The maximum value (inclusive)</param>
	/// <returns>The validation builder for method chaining</returns>
	public static IStandaloneValidationBuilder<T> Between<T>( this IStandaloneValidationBuilder<T> builder, T min, T max ) where T : struct, IComparable<T> {
		return builder.AddRule( new BetweenRule<T>( min, max ) );
	}

	#endregion

	#region Enum Validation Extensions

	/// <summary>
	/// Adds a validation rule that the enum value must be a valid defined enum value
	/// </summary>
	/// <param name="builder">The validation builder</param>
	/// <returns>The validation builder for method chaining</returns>
	public static IStandaloneValidationBuilder<TEnum> IsValidEnumValue<TEnum>( this IStandaloneValidationBuilder<TEnum> builder ) where TEnum : struct, Enum {
		return builder.AddRule( new IsValidEnumValueRule<TEnum>( ) );
	}

	#endregion

	#region Constant Validation Extensions

	/// <summary>
	/// Adds a validation rule that the value exists within a Constant type definition
	/// </summary>
	/// <param name="builder">The validation builder</param>
	/// <returns>The validation builder for method chaining</returns>
	public static IStandaloneValidationBuilder<TValue> ExistsInConstant<TConstant, TValue>( this IStandaloneValidationBuilder<TValue> builder )
		where TConstant : Constant<TConstant, TValue>
		where TValue : IEquatable<TValue>, IComparable<TValue> {
		return builder.AddRule( new ValueExistsInConstantRule<TConstant, TValue>( ) );
	}

	#endregion

	#region Collection Validation Extensions

	/// <summary>
	/// Adds a validation rule that the collection must not be empty
	/// </summary>
	/// <param name="builder">The validation builder</param>
	/// <returns>The validation builder for method chaining</returns>
	public static IStandaloneValidationBuilder<IEnumerable<T>> NotEmpty<T>( this IStandaloneValidationBuilder<IEnumerable<T>> builder ) {
		return builder.AddRule( new NotEmptyCollectionRule<T>( ) );
	}

	/// <summary>
	/// Adds a validation rule that the collection count must be between the specified bounds
	/// </summary>
	/// <param name="builder">The validation builder</param>
	/// <param name="min">The minimum count (inclusive)</param>
	/// <param name="max">The maximum count (inclusive)</param>
	/// <returns>The validation builder for method chaining</returns>
	public static IStandaloneValidationBuilder<IEnumerable<T>> CountBetween<T>( this IStandaloneValidationBuilder<IEnumerable<T>> builder, int min, int max ) {
		return builder.AddRule( new CountBetweenRule<T>( min, max ) );
	}

	#endregion

	#region Internal Helper Methods

	/// <summary>
	/// Internal helper method to add rules to the standalone validation builder
	/// </summary>
	/// <typeparam name="T">The value type</typeparam>
	/// <param name="builder">The validation builder</param>
	/// <param name="rule">The validation rule to add</param>
	/// <returns>The validation builder for method chaining</returns>
	internal static IStandaloneValidationBuilder<T> AddRule<T>( this IStandaloneValidationBuilder<T> builder, Myth.Rules.Base.ValidationRuleBase<T> rule ) {
		if ( builder is StandaloneValidationBuilder<T> concreteBuilder ) {
			return concreteBuilder.AddExternalRule( rule );
		}

		return builder;
	}

	#endregion
}
