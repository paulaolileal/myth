using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Collections; 

internal sealed class AnyRule<T> : ValidationRuleBase<IEnumerable<T>> {
	private readonly Func<T, bool> _predicate;

	public AnyRule( Func<T, bool> predicate ) {
		_predicate = predicate;
	}

	protected override Task<bool> EvaluateAsync( RuleContext<IEnumerable<T>> context ) {
		return Task.FromResult( context.Value?.Any( _predicate ) == true );
	}

	protected override string GetDefaultMessage( IEnumerable<T> value ) {
		return "At least one item must match the condition";
	}
}
