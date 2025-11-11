using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Generics; 

internal sealed class BeDefaultRule<T> : ValidationRuleBase<T> {

	protected override Task<bool> EvaluateAsync( RuleContext<T> context ) {
		return Task.FromResult( EqualityComparer<T>.Default.Equals( context.Value, default ) );
	}

	protected override string GetDefaultMessage( T value ) {
		return "Value must be default";
	}
}
