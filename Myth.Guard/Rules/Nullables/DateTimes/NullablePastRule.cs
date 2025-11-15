using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Guard.Rules.Nullables.DateTimes;

/// <summary>
/// Validation rule that checks if a nullable DateTime value represents a past date/time
/// If the value is null, the validation passes
/// </summary>
internal sealed class NullablePastRule : ValidationRuleBase<DateTime?> {

	/// <summary>
	/// Evaluates whether the nullable DateTime value represents a past date/time
	/// </summary>
	/// <param name="context">The rule execution context</param>
	/// <returns>A task representing the validation result</returns>
	protected override Task<bool> EvaluateAsync( RuleContext<DateTime?> context ) {
		// If null, validation passes
		if ( !context.Value.HasValue )
			return Task.FromResult( true );

		// Check if the value is in the past
		return Task.FromResult( context.Value.Value < DateTime.Now );
	}

	/// <summary>
	/// Gets the default error message for this rule
	/// </summary>
	/// <param name="value">The value being validated</param>
	/// <returns>The default error message</returns>
	protected override string GetDefaultMessage( DateTime? value ) {
		return value.HasValue
			? $"The value '{value.Value:yyyy-MM-dd HH:mm:ss}' must be a past date/time."
			: "The value must be a past date/time.";
	}
}
