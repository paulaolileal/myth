using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Guard.Rules.Nullables.Structs;

/// <summary>
/// Validation rule that checks if a nullable struct value is not its default value.
/// If the value is null, the validation passes (use NotNull() to reject null values).
/// </summary>
internal sealed class NullableNotDefaultRule<T> : ValidationRuleBase<T?>
	where T : struct, IEquatable<T> {

	/// <summary>
	/// Evaluates whether the nullable struct value is not default.
	/// </summary>
	/// <param name="context">The rule execution context</param>
	/// <returns>A task representing the validation result</returns>
	protected override Task<bool> EvaluateAsync( RuleContext<T?> context ) {
		if ( !context.Value.HasValue )
			return Task.FromResult( true );

		return Task.FromResult( !EqualityComparer<T>.Default.Equals( context.Value.Value, default ) );
	}

	/// <summary>
	/// Gets the default error message for this rule
	/// </summary>
	/// <param name="value">The value being validated</param>
	/// <returns>The default error message</returns>
	protected override string GetDefaultMessage( T? value ) {
		return "Value must not be default";
	}
}
