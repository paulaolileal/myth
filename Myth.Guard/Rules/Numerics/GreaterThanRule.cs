using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Numerics;

internal sealed class GreaterThanRule<T> : ValidationRuleBase<T> where T : struct, IComparable<T> {
	private readonly T _min;

	public GreaterThanRule( T min ) {
		_min = min;
	}

	protected override Task<bool> EvaluateAsync( RuleContext<T> context ) {
		return Task.FromResult( context.Value.CompareTo( _min ) > 0 );
	}

	protected override string GetDefaultMessage( T value ) {
		return $"Value must be greater than {_min}";
	}
}
