using Myth.Rules.Base;

namespace Myth.Interfaces.Rules;

/// <summary>
/// Base interface for rule configurations that provides common validation rules applicable to all types.
/// </summary>
/// <typeparam name="T">The type of value being validated.</typeparam>
public interface IRuleConfigurator<T> {

	/// <summary>
	/// Validates that the value satisfies a custom predicate condition.
	/// </summary>
	/// <param name="predicate">A function that defines the validation logic. Should return true if validation passes.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.Age, r => r.Respect(age => age >= 18));
	/// </code>
	/// </example>
	FieldRuleBuilder<T> Respect( Func<T, bool> predicate );

	/// <summary>
	/// Validates that the value satisfies a custom asynchronous predicate condition with access to the service provider.
	/// This method allows for validation that requires external services like database lookups or API calls.
	/// </summary>
	/// <param name="predicate">An async function that defines the validation logic with access to cancellation token and service provider. Should return true if validation passes.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.Email, r => r.RespectAsync(async (email, ct, sp) => {
	///     var userService = sp.GetRequiredService&lt;IUserService&gt;();
	///     return await userService.IsEmailAvailableAsync(email, ct);
	/// }));
	/// </code>
	/// </example>
	FieldRuleBuilder<T> RespectAsync( Func<T, CancellationToken, IServiceProvider, Task<bool>> predicate );

	/// <summary>
	/// Validates that the value satisfies a custom predicate condition with access to the entire entity being validated.
	/// This method allows for cross-property validation within the same object.
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity being validated.</typeparam>
	/// <param name="predicate">A function that receives both the property value and the parent entity, and returns true if validation passes.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.Email, r => r.Respect&lt;User&gt;((email, user) =>
	///     user.IsActive || string.IsNullOrEmpty(email)));
	/// </code>
	/// </example>
	FieldRuleBuilder<T> Respect<TEntity>( Func<T, TEntity, bool> predicate ) where TEntity : class;

	/// <summary>
	/// Validates that the value satisfies a custom asynchronous predicate condition with access to the entire entity being validated and service provider.
	/// This method allows for cross-property validation with external services like database lookups or API calls.
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity being validated.</typeparam>
	/// <param name="predicate">An async function that receives the property value, parent entity, cancellation token, and service provider, and returns true if validation passes.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.Email, r => r.RespectAsync&lt;LoginDto&gt;(async (email, loginData, ct, sp) => {
	///     var userRepo = sp.GetRequiredService&lt;IUserRepository&gt;();
	///     return await userRepo.AnyAsync(u => u.Email == email && u.Password == loginData.Password, ct);
	/// }));
	/// </code>
	/// </example>
	FieldRuleBuilder<T> RespectAsync<TEntity>( Func<T, TEntity, CancellationToken, IServiceProvider, Task<bool>> predicate ) where TEntity : class;

	/// <summary>
	/// Validates that the value is equal to the specified expected value.
	/// </summary>
	/// <param name="value">The expected value that the field must equal.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.Status, r => r.EqualsTo("Active"));
	/// </code>
	/// </example>
	FieldRuleBuilder<T> EqualsTo( T value );

	/// <summary>
	/// Validates that the value is not equal to the specified value.
	/// </summary>
	/// <param name="value">The value that the field must not equal.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.Email, r => r.NotEqualsTo("admin@example.com"));
	/// </code>
	/// </example>
	FieldRuleBuilder<T> NotEqualsTo( T value );

	/// <summary>
	/// Validates that the value is null. This rule will pass only if the value is null.
	/// </summary>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.OptionalField, r => r.BeNull());
	/// </code>
	/// </example>
	FieldRuleBuilder<T> BeNull( );

	/// <summary>
	/// Validates that the value is not null. This rule will fail if the value is null.
	/// </summary>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.Name, r => r.NotNull());
	/// </code>
	/// </example>
	FieldRuleBuilder<T> NotNull( );

	/// <summary>
	/// Validates that the value is equal to the default value of its type (e.g., 0 for integers, null for reference types).
	/// </summary>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.Count, r => r.BeDefault()); // Validates Count == 0
	/// </code>
	/// </example>
	FieldRuleBuilder<T> BeDefault( );

	/// <summary>
	/// Validates that the value is not equal to the default value of its type.
	/// For reference types, this ensures the value is not null. For value types, this ensures it's not the zero value.
	/// </summary>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.Id, r => r.NotDefault()); // Validates Id != 0
	/// </code>
	/// </example>
	FieldRuleBuilder<T> NotDefault( );
}
