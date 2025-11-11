using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Enums;

/// <summary>
/// Validates that an enum value is a valid defined member of the enum.
/// This prevents cases where arbitrary integer values are cast to enums (e.g., (MyEnum)999).
/// </summary>
/// <typeparam name="T">The enum type to validate</typeparam>
internal sealed class IsValidEnumValueRule<T> : ValidationRuleBase<T> where T : struct, Enum {

	protected override Task<bool> EvaluateAsync( RuleContext<T> context ) {
		// Check if the value is actually defined in the enum
		return Task.FromResult( Enum.IsDefined( typeof( T ), context.Value ) );
	}

	protected override string GetDefaultMessage( T value ) {
		return $"Value {value} ({Convert.ToInt32( value )}) is not a valid {typeof( T ).Name} enum member";
	}
}
