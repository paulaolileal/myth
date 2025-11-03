using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Guard.Rules.Nullables.DateTimes;

/// <summary>
/// Validation rule that checks if a nullable DateTime value is between two specified dates/times
/// If the value is null, the validation passes
/// </summary>
internal sealed class NullableBetweenRule : ValidationRuleBase<DateTime?> {
	private readonly DateTime _start;
	private readonly DateTime _end;

	/// <summary>
	/// Initializes a new instance of the NullableBetweenRule class
	/// </summary>
	/// <param name="start">The start of the range (inclusive)</param>
	/// <param name="end">The end of the range (inclusive)</param>
	public NullableBetweenRule( DateTime start, DateTime end ) {
		_start = start;
		_end = end;
	}

	/// <summary>
	/// Evaluates whether the nullable DateTime value is between the specified range
	/// </summary>
	/// <param name="context">The rule execution context</param>
	/// <returns>A task representing the validation result</returns>
	protected override Task<bool> EvaluateAsync( RuleContext<DateTime?> context ) {
		// If null, validation passes
		if ( !context.Value.HasValue )
			return Task.FromResult( true );

		// Check if the value is between the range (inclusive)
		var value = context.Value.Value;
		return Task.FromResult( value >= _start && value <= _end );
	}

	/// <summary>
	/// Gets the default error message for this rule
	/// </summary>
	/// <param name="value">The value being validated</param>
	/// <returns>The default error message</returns>
	protected override string GetDefaultMessage( DateTime? value ) {
		return value.HasValue
			? $"The value '{value.Value:yyyy-MM-dd HH:mm:ss}' must be between '{_start:yyyy-MM-dd HH:mm:ss}' and '{_end:yyyy-MM-dd HH:mm:ss}'."
			: $"The value must be between '{_start:yyyy-MM-dd HH:mm:ss}' and '{_end:yyyy-MM-dd HH:mm:ss}'.";
	}
}