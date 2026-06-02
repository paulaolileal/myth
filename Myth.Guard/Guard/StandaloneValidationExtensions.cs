using Myth.Rules.Collections;
using Myth.Rules.Constants;
using Myth.Rules.DateTimes;
using Myth.Rules.Dictionaries;
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
	/// Alias for <see cref="MinimumLength"/> — validates that the string length is at least the specified minimum.
	/// </summary>
	public static IStandaloneValidationBuilder<string> MinLength( this IStandaloneValidationBuilder<string> builder, int length ) =>
		builder.MinimumLength( length );

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
	/// Alias for <see cref="MaximumLength"/> — validates that the string length does not exceed the specified maximum.
	/// </summary>
	public static IStandaloneValidationBuilder<string> MaxLength( this IStandaloneValidationBuilder<string> builder, int length ) =>
		builder.MaximumLength( length );

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

	#region Dictionary Validation Extensions

	/// <summary>
	/// Adds a validation rule that the dictionary must not be empty
	/// </summary>
	/// <param name="builder">The validation builder</param>
	/// <returns>The validation builder for method chaining</returns>
	public static IStandaloneValidationBuilder<IDictionary<TKey, TValue>> NotEmpty<TKey, TValue>( this IStandaloneValidationBuilder<IDictionary<TKey, TValue>> builder ) {
		return builder.AddRule( new NotEmptyDictionaryRule<TKey, TValue>( ) );
	}

	/// <summary>
	/// Adds a validation rule that the dictionary count must be greater than the specified value
	/// </summary>
	/// <param name="builder">The validation builder</param>
	/// <param name="min">The minimum count (exclusive)</param>
	/// <returns>The validation builder for method chaining</returns>
	public static IStandaloneValidationBuilder<IDictionary<TKey, TValue>> CountGreaterThan<TKey, TValue>( this IStandaloneValidationBuilder<IDictionary<TKey, TValue>> builder, int min ) {
		return builder.AddRule( new DictionaryCountGreaterThanRule<TKey, TValue>( min ) );
	}

	/// <summary>
	/// Adds a validation rule that the dictionary count must be less than the specified value
	/// </summary>
	/// <param name="builder">The validation builder</param>
	/// <param name="max">The maximum count (exclusive)</param>
	/// <returns>The validation builder for method chaining</returns>
	public static IStandaloneValidationBuilder<IDictionary<TKey, TValue>> CountLessThan<TKey, TValue>( this IStandaloneValidationBuilder<IDictionary<TKey, TValue>> builder, int max ) {
		return builder.AddRule( new DictionaryCountLessThanRule<TKey, TValue>( max ) );
	}

	/// <summary>
	/// Adds a validation rule that the dictionary count must be between the specified bounds
	/// </summary>
	/// <param name="builder">The validation builder</param>
	/// <param name="min">The minimum count (inclusive)</param>
	/// <param name="max">The maximum count (inclusive)</param>
	/// <returns>The validation builder for method chaining</returns>
	public static IStandaloneValidationBuilder<IDictionary<TKey, TValue>> CountBetween<TKey, TValue>( this IStandaloneValidationBuilder<IDictionary<TKey, TValue>> builder, int min, int max ) {
		return builder.AddRule( new DictionaryCountBetweenRule<TKey, TValue>( min, max ) );
	}

	/// <summary>
	/// Adds a validation rule that the dictionary must contain the specified key
	/// </summary>
	/// <param name="builder">The validation builder</param>
	/// <param name="key">The key that must exist in the dictionary</param>
	/// <returns>The validation builder for method chaining</returns>
	public static IStandaloneValidationBuilder<IDictionary<TKey, TValue>> ContainsKey<TKey, TValue>( this IStandaloneValidationBuilder<IDictionary<TKey, TValue>> builder, TKey key ) {
		return builder.AddRule( new ContainsKeyRule<TKey, TValue>( key ) );
	}

	/// <summary>
	/// Adds a validation rule that the dictionary must not contain the specified key
	/// </summary>
	/// <param name="builder">The validation builder</param>
	/// <param name="key">The key that must not exist in the dictionary</param>
	/// <returns>The validation builder for method chaining</returns>
	public static IStandaloneValidationBuilder<IDictionary<TKey, TValue>> NotContainsKey<TKey, TValue>( this IStandaloneValidationBuilder<IDictionary<TKey, TValue>> builder, TKey key ) {
		return builder.AddRule( new NotContainsKeyRule<TKey, TValue>( key ) );
	}

	/// <summary>
	/// Adds a validation rule that the dictionary must contain the specified value
	/// </summary>
	/// <param name="builder">The validation builder</param>
	/// <param name="value">The value that must exist in the dictionary</param>
	/// <returns>The validation builder for method chaining</returns>
	public static IStandaloneValidationBuilder<IDictionary<TKey, TValue>> ContainsValue<TKey, TValue>( this IStandaloneValidationBuilder<IDictionary<TKey, TValue>> builder, TValue value ) {
		return builder.AddRule( new ContainsValueRule<TKey, TValue>( value ) );
	}

	/// <summary>
	/// Adds a validation rule that all keys in the dictionary must satisfy a predicate
	/// </summary>
	/// <param name="builder">The validation builder</param>
	/// <param name="predicate">The predicate that all keys must satisfy</param>
	/// <returns>The validation builder for method chaining</returns>
	public static IStandaloneValidationBuilder<IDictionary<TKey, TValue>> AllKeys<TKey, TValue>( this IStandaloneValidationBuilder<IDictionary<TKey, TValue>> builder, Func<TKey, bool> predicate ) {
		return builder.AddRule( new AllKeysRule<TKey, TValue>( predicate ) );
	}

	/// <summary>
	/// Adds a validation rule that all values in the dictionary must satisfy a predicate
	/// </summary>
	/// <param name="builder">The validation builder</param>
	/// <param name="predicate">The predicate that all values must satisfy</param>
	/// <returns>The validation builder for method chaining</returns>
	public static IStandaloneValidationBuilder<IDictionary<TKey, TValue>> AllValues<TKey, TValue>( this IStandaloneValidationBuilder<IDictionary<TKey, TValue>> builder, Func<TValue, bool> predicate ) {
		return builder.AddRule( new AllValuesRule<TKey, TValue>( predicate ) );
	}

	/// <summary>
	/// Adds a validation rule that at least one key in the dictionary must satisfy a predicate
	/// </summary>
	/// <param name="builder">The validation builder</param>
	/// <param name="predicate">The predicate that at least one key must satisfy</param>
	/// <returns>The validation builder for method chaining</returns>
	public static IStandaloneValidationBuilder<IDictionary<TKey, TValue>> AnyKey<TKey, TValue>( this IStandaloneValidationBuilder<IDictionary<TKey, TValue>> builder, Func<TKey, bool> predicate ) {
		return builder.AddRule( new AnyKeyRule<TKey, TValue>( predicate ) );
	}

	/// <summary>
	/// Adds a validation rule that at least one value in the dictionary must satisfy a predicate
	/// </summary>
	/// <param name="builder">The validation builder</param>
	/// <param name="predicate">The predicate that at least one value must satisfy</param>
	/// <returns>The validation builder for method chaining</returns>
	public static IStandaloneValidationBuilder<IDictionary<TKey, TValue>> AnyValue<TKey, TValue>( this IStandaloneValidationBuilder<IDictionary<TKey, TValue>> builder, Func<TValue, bool> predicate ) {
		return builder.AddRule( new AnyValueRule<TKey, TValue>( predicate ) );
	}

	/// <summary>
	/// Adds a validation rule that no keys in the dictionary must satisfy a predicate
	/// </summary>
	/// <param name="builder">The validation builder</param>
	/// <param name="predicate">The predicate that no keys should satisfy</param>
	/// <returns>The validation builder for method chaining</returns>
	public static IStandaloneValidationBuilder<IDictionary<TKey, TValue>> NoKeys<TKey, TValue>( this IStandaloneValidationBuilder<IDictionary<TKey, TValue>> builder, Func<TKey, bool> predicate ) {
		return builder.AddRule( new NoKeysRule<TKey, TValue>( predicate ) );
	}

	/// <summary>
	/// Adds a validation rule that no values in the dictionary must satisfy a predicate
	/// </summary>
	/// <param name="builder">The validation builder</param>
	/// <param name="predicate">The predicate that no values should satisfy</param>
	/// <returns>The validation builder for method chaining</returns>
	public static IStandaloneValidationBuilder<IDictionary<TKey, TValue>> NoValues<TKey, TValue>( this IStandaloneValidationBuilder<IDictionary<TKey, TValue>> builder, Func<TValue, bool> predicate ) {
		return builder.AddRule( new NoValuesRule<TKey, TValue>( predicate ) );
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
