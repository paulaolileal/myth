using Myth.Interfaces;
using Myth.Interfaces.Rules;
using Myth.Rules.Generics;

namespace Myth.Rules.Base;

/// <summary>
/// Base implementation of <see cref="IRuleConfigurator{T}"/> that provides common validation rules
/// applicable to all types. This class serves as the foundation for type-specific rule configurators
/// and implements the core validation rule functionality.
/// </summary>
/// <typeparam name="T">The type of value being validated.</typeparam>
/// <remarks>
/// This class acts as an adapter between the fluent API and the underlying rule building system.
/// It wraps a <see cref="FieldRuleBuilder{T}"/> and provides a clean interface for adding validation rules.
/// Type-specific configurators (like string or numeric configurators) typically inherit from this class
/// to extend the available validation rules.
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="RuleConfigurator{T}"/> class with the specified builder.
/// </remarks>
/// <param name="builder">The field rule builder that will manage the validation rules.</param>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
public class RuleConfigurator<T>( FieldRuleBuilder<T> builder ) : IRuleConfigurator<T>, IRuleBuilder<T> {

	/// <summary>
	/// The underlying field rule builder that manages the collection of validation rules.
	/// </summary>
	protected readonly FieldRuleBuilder<T> Builder = builder;

	/// <summary>
	/// Validates that the value satisfies a custom predicate condition.
	/// </summary>
	/// <param name="predicate">A function that defines the validation logic. Should return true if validation passes.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.Respect(age => age >= 18); // Age must be 18 or older
	/// </code>
	/// </example>
	public FieldRuleBuilder<T> Respect( Func<T, bool> predicate ) {
		Builder.AddRule( new CustomRule<T>( predicate ) );
		return Builder;
	}

	/// <summary>
	/// Validates that the value satisfies a custom asynchronous predicate condition with access to the service provider.
	/// This method allows for validation that requires external services like database lookups or API calls.
	/// </summary>
	/// <param name="predicate">An async function that defines the validation logic with access to cancellation token and service provider. Should return true if validation passes.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.RespectAsync(async (email, ct, sp) => {
	///     var userService = sp.GetRequiredService&lt;IUserService&gt;();
	///     return await userService.IsEmailAvailableAsync(email, ct);
	/// });
	/// </code>
	/// </example>
	public FieldRuleBuilder<T> RespectAsync( Func<T, CancellationToken, IServiceProvider, Task<bool>> predicate ) {
		Builder.AddRule( new CustomRule<T>( predicate ) );
		return Builder;
	}

	/// <summary>
	/// Validates that the value satisfies a custom predicate condition with access to the entire entity being validated.
	/// This method allows for cross-property validation within the same object.
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity being validated.</typeparam>
	/// <param name="predicate">A function that receives both the property value and the parent entity, and returns true if validation passes.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.Respect&lt;User&gt;((email, user) => {
	///     return user.IsActive || string.IsNullOrEmpty(email); // Email required only for active users
	/// });
	/// </code>
	/// </example>
	public FieldRuleBuilder<T> Respect<TEntity>( Func<T, TEntity, bool> predicate ) where TEntity : class {
		var entityPredicate = new Func<T, object, bool>( ( value, entity ) => predicate( value, ( TEntity )entity ) );
		Builder.AddRule( new CustomRule<T>( entityPredicate ) );
		return Builder;
	}

	/// <summary>
	/// Validates that the value satisfies a custom asynchronous predicate condition with access to the entire entity being validated and service provider.
	/// This method allows for cross-property validation with external services like database lookups or API calls.
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity being validated.</typeparam>
	/// <param name="predicate">An async function that receives the property value, parent entity, cancellation token, and service provider, and returns true if validation passes.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.RespectAsync&lt;LoginDto&gt;(async (email, loginData, ct, sp) => {
	///     var userRepo = sp.GetRequiredService&lt;IUserRepository&gt;();
	///     return await userRepo.AnyAsync(u => u.Email == email && u.Password == loginData.Password, ct);
	/// });
	/// </code>
	/// </example>
	public FieldRuleBuilder<T> RespectAsync<TEntity>( Func<T, TEntity, CancellationToken, IServiceProvider, Task<bool>> predicate ) where TEntity : class {
		var entityPredicate = new Func<T, object, CancellationToken, IServiceProvider, Task<bool>>(
			( value, entity, ct, sp ) => predicate( value, ( TEntity )entity, ct, sp )
		);
		Builder.AddRule( new CustomRule<T>( entityPredicate ) );
		return Builder;
	}

	/// <summary>
	/// Validates that the value is equal to the specified expected value.
	/// </summary>
	/// <param name="value">The expected value that the field must equal.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.EqualsTo("Active"); // Status must be exactly "Active"
	/// </code>
	/// </example>
	public FieldRuleBuilder<T> EqualsTo( T value ) {
		Builder.AddRule( new EqualsRule<T>( value ) );
		return Builder;
	}

	/// <summary>
	/// Validates that the value is not equal to the specified value.
	/// </summary>
	/// <param name="value">The value that the field must not equal.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.NotEqualsTo("admin@example.com"); // Email cannot be the admin email
	/// </code>
	/// </example>
	public FieldRuleBuilder<T> NotEqualsTo( T value ) {
		Builder.AddRule( new NotEqualsRule<T>( value ) );
		return Builder;
	}

	/// <summary>
	/// Validates that the value is null. This rule will pass only if the value is null.
	/// </summary>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.BeNull(); // Optional field must be null
	/// </code>
	/// </example>
	public FieldRuleBuilder<T> BeNull( ) {
		Builder.AddRule( new BeNullRule<T>( ) );
		return Builder;
	}

	/// <summary>
	/// Validates that the value is not null. This rule will fail if the value is null.
	/// </summary>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.NotNull(); // Required field cannot be null
	/// </code>
	/// </example>
	public FieldRuleBuilder<T> NotNull( ) {
		Builder.AddRule( new NotNullRule<T>( ) );
		return Builder;
	}

	/// <summary>
	/// Validates that the value is equal to the default value of its type (e.g., 0 for integers, null for reference types).
	/// </summary>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.BeDefault(); // Count must be 0 (default for int)
	/// </code>
	/// </example>
	public FieldRuleBuilder<T> BeDefault( ) {
		Builder.AddRule( new BeDefaultRule<T>( ) );
		return Builder;
	}

	/// <summary>
	/// Validates that the value is not equal to the default value of its type.
	/// For reference types, this ensures the value is not null. For value types, this ensures it's not the zero value.
	/// </summary>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.NotDefault(); // ID must not be 0 (default for int)
	/// </code>
	/// </example>
	public FieldRuleBuilder<T> NotDefault( ) {
		Builder.AddRule( new NotDefaultRule<T>( ) );
		return Builder;
	}

	/// <summary>
	/// Gets the collection of configured validation rules.
	/// </summary>
	/// <returns>A read-only list of validation rules that have been configured for this field.</returns>
	/// <remarks>
	/// This method is used internally by the validation framework to retrieve and execute
	/// the configured rules during the validation process.
	/// </remarks>
	public IReadOnlyList<IValidationRule<T>> GetRules( ) {
		return Builder.GetRules( );
	}
}
