using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Numerics;

internal sealed class NegativeRule<T> : ValidationRuleBase<T> where T : struct, IComparable<T> {

	protected override Task<bool> EvaluateAsync( RuleContext<T> context ) {
		dynamic value = context.Value;
		return Task.FromResult( value < 0 );
	}

	protected override string GetDefaultMessage( T value ) {
		return "Value must be negative";
	}
}
