using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Collections;

internal sealed class CountGreaterThanRule<T> : ValidationRuleBase<IEnumerable<T>> {
	private readonly int _min;

	public CountGreaterThanRule( int min ) {
		_min = min;
	}

	protected override Task<bool> EvaluateAsync( RuleContext<IEnumerable<T>> context ) {
		return Task.FromResult( context.Value?.Count( ) > _min );
	}

	protected override string GetDefaultMessage( IEnumerable<T> value ) {
		return $"Collection must have more than {_min} items";
	}
}
