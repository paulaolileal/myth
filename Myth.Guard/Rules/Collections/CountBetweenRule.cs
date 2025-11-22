using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Collections;

internal sealed class CountBetweenRule<T>( int min, int max ) : ValidationRuleBase<IEnumerable<T>> {
	private readonly int _min = min;
	private readonly int _max = max;

	protected override Task<bool> EvaluateAsync( RuleContext<IEnumerable<T>> context ) {
		var count = context.Value?.Count( ) ?? 0;
		return Task.FromResult( count >= _min && count <= _max );
	}

	protected override string GetDefaultMessage( IEnumerable<T> value ) {
		return $"Collection must have between {_min} and {_max} items";
	}
}
