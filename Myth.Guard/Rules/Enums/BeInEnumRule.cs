using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Enums;

internal sealed class BeInEnumRule<T> : ValidationRuleBase<T> where T : struct, Enum {

	protected override Task<bool> EvaluateAsync( RuleContext<T> context ) {
		return Task.FromResult( Enum.IsDefined( typeof( T ), context.Value ) );
	}

	protected override string GetDefaultMessage( T value ) {
		return $"Value {value} is not a valid {typeof( T ).Name}";
	}
}
