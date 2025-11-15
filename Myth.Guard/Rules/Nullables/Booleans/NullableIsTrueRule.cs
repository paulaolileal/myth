using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Guard.Rules.Nullables.Booleans;

/// <summary>
/// Validation rule that checks if a nullable boolean value is true
/// If the value is null, the validation passes
/// </summary>
internal sealed class NullableIsTrueRule : ValidationRuleBase<bool?> {

	/// <summary>
	/// Evaluates whether the nullable boolean value is true
	/// </summary>
	/// <param name="context">The rule execution context</param>
	/// <returns>A task representing the validation result</returns>
	protected override Task<bool> EvaluateAsync( RuleContext<bool?> context ) {
		// If null, validation passes
		if ( !context.Value.HasValue )
			return Task.FromResult( true );

		// Check if the value is true
		return Task.FromResult( context.Value.Value == true );
	}

	/// <summary>
	/// Gets the default error message for this rule
	/// </summary>
	/// <param name="value">The value being validated</param>
	/// <returns>The default error message</returns>
	protected override string GetDefaultMessage( bool? value ) {
		return value.HasValue
			? $"The value '{value.Value}' must be true."
			: "The value must be true.";
	}
}
