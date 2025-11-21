using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Collections;

internal sealed class NoneRule<T>( Func<T, bool> predicate ) : ValidationRuleBase<IEnumerable<T>> {
	private readonly Func<T, bool> _predicate = predicate;

	protected override Task<bool> EvaluateAsync( RuleContext<IEnumerable<T>> context ) {
		return Task.FromResult( context.Value?.Any( _predicate ) == false );
	}

	protected override string GetDefaultMessage( IEnumerable<T> value ) {
		return "No item must match the condition";
	}
}
