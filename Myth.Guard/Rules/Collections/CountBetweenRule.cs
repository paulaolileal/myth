using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Collections;

internal sealed class CountBetweenRule<T> : ValidationRuleBase<IEnumerable<T>> {
	private readonly int _min;
	private readonly int _max;

	public CountBetweenRule( int min, int max ) {
		_min = min;
		_max = max;
	}

	protected override Task<bool> EvaluateAsync( RuleContext<IEnumerable<T>> context ) {
		var count = context.Value?.Count( ) ?? 0;
		return Task.FromResult( count >= _min && count <= _max );
	}

	protected override string GetDefaultMessage( IEnumerable<T> value ) {
		return $"Collection must have between {_min} and {_max} items";
	}
}
