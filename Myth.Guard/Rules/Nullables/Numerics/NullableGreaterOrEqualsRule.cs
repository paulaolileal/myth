using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Guard.Rules.Nullables.Numerics;

/// <summary>
/// Validation rule that checks if a nullable numeric value is greater than or equal to a specified value
/// If the value is null, the validation passes
/// </summary>
/// <typeparam name="T">The numeric type</typeparam>
internal sealed class NullableGreaterOrEqualsRule<T> : ValidationRuleBase<T?>
	where T : struct, IComparable<T> {
	private readonly T _compareValue;

	/// <summary>
	/// Initializes a new instance of the NullableGreaterOrEqualsRule class
	/// </summary>
	/// <param name="compareValue">The value to compare against</param>
	public NullableGreaterOrEqualsRule( T compareValue ) {
		_compareValue = compareValue;
	}

	/// <summary>
	/// Evaluates whether the nullable numeric value is greater than or equal to the specified value
	/// </summary>
	/// <param name="context">The rule execution context</param>
	/// <returns>A task representing the validation result</returns>
	protected override Task<bool> EvaluateAsync( RuleContext<T?> context ) {
		// If null, validation passes
		if ( !context.Value.HasValue )
			return Task.FromResult( true );

		// Check if the value is greater than or equal to the comparison value
		return Task.FromResult( context.Value.Value.CompareTo( _compareValue ) >= 0 );
	}

	/// <summary>
	/// Gets the default error message for this rule
	/// </summary>
	/// <param name="value">The value being validated</param>
	/// <returns>The default error message</returns>
	protected override string GetDefaultMessage( T? value ) {
		return value.HasValue
			? $"The value '{value.Value}' must be greater than or equal to '{_compareValue}'."
			: $"The value must be greater than or equal to '{_compareValue}'.";
	}
}
