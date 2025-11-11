using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Numerics; 

internal sealed class BetweenRule<T> : ValidationRuleBase<T> where T : struct, IComparable<T> {
	private readonly T _min;
	private readonly T _max;

	public BetweenRule( T min, T max ) {
		_min = min;
		_max = max;
	}

	protected override Task<bool> EvaluateAsync( RuleContext<T> context ) {
		return Task.FromResult( context.Value.CompareTo( _min ) >= 0 && context.Value.CompareTo( _max ) <= 0 );
	}

	protected override string GetDefaultMessage( T value ) {
		return $"Value must be between {_min} and {_max}";
	}
}
