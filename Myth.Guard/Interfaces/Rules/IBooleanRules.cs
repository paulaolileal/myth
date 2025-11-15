using Myth.Rules.Base;

namespace Myth.Interfaces.Rules;

/// <summary>
/// Provides validation rules specifically designed for boolean values.
/// </summary>
public interface IBooleanRules : IRuleConfigurator<bool> {

	/// <summary>
	/// Validates that the boolean value is true.
	/// </summary>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.IsAccepted, r => r.IsTrue()); // Must be accepted
	/// builder.For(x => x.IsActive, r => r.IsTrue()); // Must be active
	/// </code>
	/// </example>
	FieldRuleBuilder<bool> IsTrue( );

	/// <summary>
	/// Validates that the boolean value is false.
	/// </summary>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.IsDeleted, r => r.IsFalse()); // Must not be deleted
	/// builder.For(x => x.IsBlocked, r => r.IsFalse()); // Must not be blocked
	/// </code>
	/// </example>
	FieldRuleBuilder<bool> IsFalse( );

	/// <summary>
	/// Validates that the boolean value is the opposite of its current value (negation).
	/// This rule will always fail as it creates an impossible condition.
	/// </summary>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.SomeFlag, r => r.Not()); // Will always fail
	/// </code>
	/// </example>
	FieldRuleBuilder<bool> Not( );
}
