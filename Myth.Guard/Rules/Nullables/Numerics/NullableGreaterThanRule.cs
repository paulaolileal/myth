using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Guard.Rules.Nullables.Numerics;

/// <summary>
/// Validation rule that checks if a nullable numeric value is greater than a specified value
/// If the value is null, the validation passes
/// </summary>
/// <typeparam name="T">The numeric type</typeparam>
/// <remarks>
/// Initializes a new instance of the NullableGreaterThanRule class
/// </remarks>
/// <param name="compareValue">The value to compare against</param>
internal sealed class NullableGreaterThanRule<T>( T compareValue ) : ValidationRuleBase<T?>
	where T : struct, IComparable<T> {
	private readonly T _compareValue = compareValue;

	/// <summary>
	/// Evaluates whether the nullable numeric value is greater than the specified value
	/// </summary>
	/// <param name="context">The rule execution context</param>
	/// <returns>A task representing the validation result</returns>
	protected override Task<bool> EvaluateAsync( RuleContext<T?> context ) {
		// If null, validation passes
		if ( !context.Value.HasValue )
			return Task.FromResult( true );

		// Check if the value is greater than the comparison value
		return Task.FromResult( context.Value.Value.CompareTo( _compareValue ) > 0 );
	}

	/// <summary>
	/// Gets the default error message for this rule
	/// </summary>
	/// <param name="value">The value being validated</param>
	/// <returns>The default error message</returns>
	protected override string GetDefaultMessage( T? value ) {
		return value.HasValue
			? $"The value '{value.Value}' must be greater than '{_compareValue}'."
			: $"The value must be greater than '{_compareValue}'.";
	}
}
