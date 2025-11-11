using Myth.Rules.Base;

namespace Myth.Interfaces.Rules;

/// <summary>
/// Provides validation rules specifically designed for collections and enumerable types,
/// including size validation, element validation, and uniqueness validation.
/// </summary>
/// <typeparam name="T">The type of elements in the collection.</typeparam>
public interface ICollectionRules<T> : IRuleConfigurator<IEnumerable<T>> {

	/// <summary>
	/// Validates that the collection is not null and contains at least one element.
	/// </summary>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.Tags, r => r.NotEmpty()); // Tags collection must not be empty
	/// builder.For(x => x.Items, r => r.NotEmpty()); // Items list must have elements
	/// </code>
	/// </example>
	FieldRuleBuilder<IEnumerable<T>> NotEmpty( );

	/// <summary>
	/// Validates that the collection contains more than the specified minimum number of elements.
	/// </summary>
	/// <param name="min">The minimum count (exclusive). The collection must have more elements than this.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <exception cref="ArgumentException">Thrown when min is negative.</exception>
	/// <example>
	/// <code>
	/// builder.For(x => x.Participants, r => r.CountGreaterThan(1)); // Must have more than 1 participant
	/// builder.For(x => x.Options, r => r.CountGreaterThan(0)); // Must have at least 1 option
	/// </code>
	/// </example>
	FieldRuleBuilder<IEnumerable<T>> CountGreaterThan( int min );

	/// <summary>
	/// Validates that the collection contains fewer than the specified maximum number of elements.
	/// </summary>
	/// <param name="max">The maximum count (exclusive). The collection must have fewer elements than this.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <exception cref="ArgumentException">Thrown when max is negative.</exception>
	/// <example>
	/// <code>
	/// builder.For(x => x.ErrorMessages, r => r.CountLessThan(10)); // Must have fewer than 10 errors
	/// builder.For(x => x.Categories, r => r.CountLessThan(5)); // Must have fewer than 5 categories
	/// </code>
	/// </example>
	FieldRuleBuilder<IEnumerable<T>> CountLessThan( int max );

	/// <summary>
	/// Validates that the collection contains a number of elements within the specified range (inclusive).
	/// </summary>
	/// <param name="min">The minimum count (inclusive).</param>
	/// <param name="max">The maximum count (inclusive).</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <exception cref="ArgumentException">Thrown when min is negative, max is negative, or min is greater than max.</exception>
	/// <example>
	/// <code>
	/// builder.For(x => x.Skills, r => r.CountBetween(1, 10)); // Must have 1-10 skills
	/// builder.For(x => x.Attachments, r => r.CountBetween(0, 5)); // Must have 0-5 attachments
	/// </code>
	/// </example>
	FieldRuleBuilder<IEnumerable<T>> CountBetween( int min, int max );

	/// <summary>
	/// Validates that all elements in the collection satisfy the specified predicate condition.
	/// </summary>
	/// <param name="predicate">A function that defines the condition each element must satisfy.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <exception cref="ArgumentNullException">Thrown when predicate is null.</exception>
	/// <example>
	/// <code>
	/// builder.For(x => x.Scores, r => r.All(score => score >= 0)); // All scores must be non-negative
	/// builder.For(x => x.Users, r => r.All(user => !string.IsNullOrEmpty(user.Email))); // All users must have email
	/// </code>
	/// </example>
	FieldRuleBuilder<IEnumerable<T>> All( Func<T, bool> predicate );

	/// <summary>
	/// Validates that at least one element in the collection satisfies the specified predicate condition.
	/// </summary>
	/// <param name="predicate">A function that defines the condition at least one element must satisfy.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <exception cref="ArgumentNullException">Thrown when predicate is null.</exception>
	/// <example>
	/// <code>
	/// builder.For(x => x.Permissions, r => r.Any(p => p.IsAdmin)); // At least one permission must be admin
	/// builder.For(x => x.Products, r => r.Any(p => p.IsAvailable)); // At least one product must be available
	/// </code>
	/// </example>
	FieldRuleBuilder<IEnumerable<T>> Any( Func<T, bool> predicate );

	/// <summary>
	/// Validates that no elements in the collection satisfy the specified predicate condition.
	/// </summary>
	/// <param name="predicate">A function that defines the condition no element should satisfy.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <exception cref="ArgumentNullException">Thrown when predicate is null.</exception>
	/// <example>
	/// <code>
	/// builder.For(x => x.Users, r => r.None(user => user.IsBanned)); // No users should be banned
	/// builder.For(x => x.Items, r => r.None(item => item.IsDeleted)); // No items should be deleted
	/// </code>
	/// </example>
	FieldRuleBuilder<IEnumerable<T>> None( Func<T, bool> predicate );

	/// <summary>
	/// Validates that all elements in the collection are unique (no duplicates).
	/// Uses the default equality comparer for type T.
	/// </summary>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.UniqueIds, r => r.Distinct()); // All IDs must be unique
	/// builder.For(x => x.Categories, r => r.Distinct()); // All categories must be unique
	/// </code>
	/// </example>
	FieldRuleBuilder<IEnumerable<T>> Distinct( );

	/// <summary>
	/// Validates that all elements in the collection are unique based on the specified key selector.
	/// No two elements should have the same key value.
	/// </summary>
	/// <typeparam name="TKey">The type of the key used for uniqueness comparison.</typeparam>
	/// <param name="keySelector">A function that extracts the key from each element for uniqueness comparison.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <exception cref="ArgumentNullException">Thrown when keySelector is null.</exception>
	/// <example>
	/// <code>
	/// builder.For(x => x.Users, r => r.DistinctBy(user => user.Email)); // All users must have unique emails
	/// builder.For(x => x.Products, r => r.DistinctBy(p => p.Sku)); // All products must have unique SKUs
	/// </code>
	/// </example>
	FieldRuleBuilder<IEnumerable<T>> DistinctBy<TKey>( Func<T, TKey> keySelector );
}
