using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Collections;

internal sealed class CountLessThanRule<T>( int max ) : ValidationRuleBase<IEnumerable<T>> {
	private readonly int _max = max;

	protected override Task<bool> EvaluateAsync( RuleContext<IEnumerable<T>> context ) {
		return Task.FromResult( context.Value?.Count( ) < _max );
	}

	protected override string GetDefaultMessage( IEnumerable<T> value ) {
		return $"Collection must have less than {_max} items";
	}
}
