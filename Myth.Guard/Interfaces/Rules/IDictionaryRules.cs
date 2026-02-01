using Myth.Rules.Base;

namespace Myth.Interfaces.Rules;

/// <summary>
/// Provides validation rules specifically designed for dictionary types,
/// including key validation, value validation, size validation, and content validation.
/// </summary>
/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
public interface IDictionaryRules<TKey, TValue> : IRuleConfigurator<IDictionary<TKey, TValue>> {

	/// <summary>
	/// Validates that the dictionary is not null and contains at least one entry.
	/// </summary>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.Settings, r => r.NotEmpty()); // Settings dictionary must not be empty
	/// builder.For(x => x.Metadata, r => r.NotEmpty()); // Metadata must have entries
	/// </code>
	/// </example>
	FieldRuleBuilder<IDictionary<TKey, TValue>> NotEmpty( );

	/// <summary>
	/// Validates that the dictionary contains more than the specified minimum number of entries.
	/// </summary>
	/// <param name="min">The minimum count (exclusive). The dictionary must have more entries than this.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <exception cref="ArgumentException">Thrown when min is negative.</exception>
	/// <example>
	/// <code>
	/// builder.For(x => x.Headers, r => r.CountGreaterThan(0)); // Must have at least 1 header
	/// builder.For(x => x.Properties, r => r.CountGreaterThan(5)); // Must have more than 5 properties
	/// </code>
	/// </example>
	FieldRuleBuilder<IDictionary<TKey, TValue>> CountGreaterThan( int min );

	/// <summary>
	/// Validates that the dictionary contains fewer than the specified maximum number of entries.
	/// </summary>
	/// <param name="max">The maximum count (exclusive). The dictionary must have fewer entries than this.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <exception cref="ArgumentException">Thrown when max is negative.</exception>
	/// <example>
	/// <code>
	/// builder.For(x => x.Tags, r => r.CountLessThan(20)); // Must have fewer than 20 tags
	/// builder.For(x => x.Attributes, r => r.CountLessThan(100)); // Must have fewer than 100 attributes
	/// </code>
	/// </example>
	FieldRuleBuilder<IDictionary<TKey, TValue>> CountLessThan( int max );

	/// <summary>
	/// Validates that the dictionary contains a number of entries within the specified range (inclusive).
	/// </summary>
	/// <param name="min">The minimum count (inclusive).</param>
	/// <param name="max">The maximum count (inclusive).</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <exception cref="ArgumentException">Thrown when min is negative, max is negative, or min is greater than max.</exception>
	/// <example>
	/// <code>
	/// builder.For(x => x.Config, r => r.CountBetween(1, 10)); // Must have 1-10 config entries
	/// builder.For(x => x.Options, r => r.CountBetween(0, 50)); // Must have 0-50 options
	/// </code>
	/// </example>
	FieldRuleBuilder<IDictionary<TKey, TValue>> CountBetween( int min, int max );

	/// <summary>
	/// Validates that the dictionary contains the specified key.
	/// </summary>
	/// <param name="key">The key that must exist in the dictionary.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <exception cref="ArgumentNullException">Thrown when key is null.</exception>
	/// <example>
	/// <code>
	/// builder.For(x => x.Headers, r => r.ContainsKey("Authorization")); // Must have Authorization header
	/// builder.For(x => x.Config, r => r.ContainsKey("ApiKey")); // Must have ApiKey in config
	/// </code>
	/// </example>
	FieldRuleBuilder<IDictionary<TKey, TValue>> ContainsKey( TKey key );

	/// <summary>
	/// Validates that the dictionary does not contain the specified key.
	/// </summary>
	/// <param name="key">The key that must not exist in the dictionary.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <exception cref="ArgumentNullException">Thrown when key is null.</exception>
	/// <example>
	/// <code>
	/// builder.For(x => x.Headers, r => r.NotContainsKey("X-Debug")); // Must not have debug header
	/// builder.For(x => x.Config, r => r.NotContainsKey("Deprecated")); // Must not have deprecated config
	/// </code>
	/// </example>
	FieldRuleBuilder<IDictionary<TKey, TValue>> NotContainsKey( TKey key );

	/// <summary>
	/// Validates that the dictionary contains the specified value.
	/// </summary>
	/// <param name="value">The value that must exist in the dictionary.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.Permissions, r => r.ContainsValue("admin")); // Must have admin permission
	/// builder.For(x => x.Roles, r => r.ContainsValue("owner")); // Must have owner role
	/// </code>
	/// </example>
	FieldRuleBuilder<IDictionary<TKey, TValue>> ContainsValue( TValue value );

	/// <summary>
	/// Validates that all keys in the dictionary satisfy the specified predicate condition.
	/// </summary>
	/// <param name="predicate">A function that defines the condition each key must satisfy.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <exception cref="ArgumentNullException">Thrown when predicate is null.</exception>
	/// <example>
	/// <code>
	/// builder.For(x => x.Headers, r => r.AllKeys(k => !string.IsNullOrEmpty(k))); // All header keys must not be empty
	/// builder.For(x => x.Config, r => r.AllKeys(k => k.Length > 3)); // All config keys must be longer than 3 chars
	/// </code>
	/// </example>
	FieldRuleBuilder<IDictionary<TKey, TValue>> AllKeys( Func<TKey, bool> predicate );

	/// <summary>
	/// Validates that all values in the dictionary satisfy the specified predicate condition.
	/// </summary>
	/// <param name="predicate">A function that defines the condition each value must satisfy.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <exception cref="ArgumentNullException">Thrown when predicate is null.</exception>
	/// <example>
	/// <code>
	/// builder.For(x => x.Scores, r => r.AllValues(v => v >= 0)); // All scores must be non-negative
	/// builder.For(x => x.Urls, r => r.AllValues(v => Uri.IsWellFormedUriString(v, UriKind.Absolute))); // All URLs must be valid
	/// </code>
	/// </example>
	FieldRuleBuilder<IDictionary<TKey, TValue>> AllValues( Func<TValue, bool> predicate );

	/// <summary>
	/// Validates that at least one key in the dictionary satisfies the specified predicate condition.
	/// </summary>
	/// <param name="predicate">A function that defines the condition at least one key must satisfy.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <exception cref="ArgumentNullException">Thrown when predicate is null.</exception>
	/// <example>
	/// <code>
	/// builder.For(x => x.Headers, r => r.AnyKey(k => k.StartsWith("X-Custom"))); // At least one custom header
	/// builder.For(x => x.Config, r => r.AnyKey(k => k.Contains("Feature"))); // At least one feature config
	/// </code>
	/// </example>
	FieldRuleBuilder<IDictionary<TKey, TValue>> AnyKey( Func<TKey, bool> predicate );

	/// <summary>
	/// Validates that at least one value in the dictionary satisfies the specified predicate condition.
	/// </summary>
	/// <param name="predicate">A function that defines the condition at least one value must satisfy.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <exception cref="ArgumentNullException">Thrown when predicate is null.</exception>
	/// <example>
	/// <code>
	/// builder.For(x => x.Permissions, r => r.AnyValue(v => v == "write")); // At least one write permission
	/// builder.For(x => x.Flags, r => r.AnyValue(v => v == true)); // At least one flag enabled
	/// </code>
	/// </example>
	FieldRuleBuilder<IDictionary<TKey, TValue>> AnyValue( Func<TValue, bool> predicate );

	/// <summary>
	/// Validates that no keys in the dictionary satisfy the specified predicate condition.
	/// </summary>
	/// <param name="predicate">A function that defines the condition no key should satisfy.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <exception cref="ArgumentNullException">Thrown when predicate is null.</exception>
	/// <example>
	/// <code>
	/// builder.For(x => x.Headers, r => r.NoKeys(k => k.Contains("Debug"))); // No debug headers allowed
	/// builder.For(x => x.Config, r => r.NoKeys(k => k.StartsWith("_"))); // No private config keys
	/// </code>
	/// </example>
	FieldRuleBuilder<IDictionary<TKey, TValue>> NoKeys( Func<TKey, bool> predicate );

	/// <summary>
	/// Validates that no values in the dictionary satisfy the specified predicate condition.
	/// </summary>
	/// <param name="predicate">A function that defines the condition no value should satisfy.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <exception cref="ArgumentNullException">Thrown when predicate is null.</exception>
	/// <example>
	/// <code>
	/// builder.For(x => x.Permissions, r => r.NoValues(v => v == "root")); // No root permissions allowed
	/// builder.For(x => x.Flags, r => r.NoValues(v => v == null)); // No null flag values
	/// </code>
	/// </example>
	FieldRuleBuilder<IDictionary<TKey, TValue>> NoValues( Func<TValue, bool> predicate );
}
