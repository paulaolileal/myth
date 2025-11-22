using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Guard.Rules.Nullables.DateTimes;

/// <summary>
/// Validation rule that checks if a nullable DateTime value is after or equal to a specified date/time
/// If the value is null, the validation passes
/// </summary>
/// <remarks>
/// Initializes a new instance of the NullableAfterOrEqualsRule class
/// </remarks>
/// <param name="compareDate">The date/time to compare against</param>
internal sealed class NullableAfterOrEqualsRule( DateTime compareDate ) : ValidationRuleBase<DateTime?> {
	private readonly DateTime _compareDate = compareDate;

	/// <summary>
	/// Evaluates whether the nullable DateTime value is after or equal to the specified date/time
	/// </summary>
	/// <param name="context">The rule execution context</param>
	/// <returns>A task representing the validation result</returns>
	protected override Task<bool> EvaluateAsync( RuleContext<DateTime?> context ) {
		// If null, validation passes
		if ( !context.Value.HasValue )
			return Task.FromResult( true );

		// Check if the value is after or equal to the comparison date
		return Task.FromResult( context.Value.Value >= _compareDate );
	}

	/// <summary>
	/// Gets the default error message for this rule
	/// </summary>
	/// <param name="value">The value being validated</param>
	/// <returns>The default error message</returns>
	protected override string GetDefaultMessage( DateTime? value ) {
		return value.HasValue
			? $"The value '{value.Value:yyyy-MM-dd HH:mm:ss}' must be after or equal to '{_compareDate:yyyy-MM-dd HH:mm:ss}'."
			: $"The value must be after or equal to '{_compareDate:yyyy-MM-dd HH:mm:ss}'.";
	}
}
