using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Numerics; 

internal sealed class LessThanRule<T> : ValidationRuleBase<T> where T : struct, IComparable<T> {
	private readonly T _max;

	public LessThanRule( T max ) {
		_max = max;
	}

	protected override Task<bool> EvaluateAsync( RuleContext<T> context ) {
		return Task.FromResult( context.Value.CompareTo( _max ) < 0 );
	}

	protected override string GetDefaultMessage( T value ) {
		return $"Value must be less than {_max}";
	}
}
