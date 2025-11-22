using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Collections;

internal sealed class AllRule<T>( Func<T, bool> predicate ) : ValidationRuleBase<IEnumerable<T>> {
	private readonly Func<T, bool> _predicate = predicate;

	protected override Task<bool> EvaluateAsync( RuleContext<IEnumerable<T>> context ) {
		return Task.FromResult( context.Value?.All( _predicate ) == true );
	}

	protected override string GetDefaultMessage( IEnumerable<T> value ) {
		return "All items must match the condition";
	}
}
