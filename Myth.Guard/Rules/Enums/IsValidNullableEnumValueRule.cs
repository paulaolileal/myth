using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Enums {

	/// <summary>
	/// Validates that a nullable enum value, when not null, is a valid defined member of the enum
	/// </summary>
	/// <typeparam name="T">The enum type to validate</typeparam>
	internal sealed class IsValidNullableEnumValueRule<T> : ValidationRuleBase<T?> where T : struct, Enum {

		protected override Task<bool> EvaluateAsync( RuleContext<T?> context ) {
			// Null values are considered valid - use NotNull() separately if needed
			if ( !context.Value.HasValue ) {
				return Task.FromResult( true );
			}

			// Check if the actual value is a defined enum member
			return Task.FromResult( Enum.IsDefined( typeof( T ), context.Value.Value ) );
		}

		protected override string GetDefaultMessage( T? value ) {
			if ( !value.HasValue ) {
				return $"Nullable enum value is null";
			}

			return $"Value {value.Value} is not a valid {typeof( T ).Name} enum member";
		}
	}
}