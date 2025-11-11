using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Collections; 

internal sealed class AllRule<T> : ValidationRuleBase<IEnumerable<T>> {
	private readonly Func<T, bool> _predicate;

	public AllRule( Func<T, bool> predicate ) {
		_predicate = predicate;
	}

	protected override Task<bool> EvaluateAsync( RuleContext<IEnumerable<T>> context ) {
		return Task.FromResult( context.Value?.All( _predicate ) == true );
	}

	protected override string GetDefaultMessage( IEnumerable<T> value ) {
		return "All items must match the condition";
	}
}
