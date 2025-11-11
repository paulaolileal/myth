using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Collections; 

internal sealed class NotEmptyCollectionRule<T> : ValidationRuleBase<IEnumerable<T>> {

	protected override Task<bool> EvaluateAsync( RuleContext<IEnumerable<T>> context ) {
		return Task.FromResult( context.Value?.Any( ) == true );
	}

	protected override string GetDefaultMessage( IEnumerable<T> value ) {
		return "Collection cannot be empty";
	}
}
