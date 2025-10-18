using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Generics {

	internal sealed class BeNullRule<T> : ValidationRuleBase<T> {

		protected override Task<bool> EvaluateAsync( RuleContext<T> context ) {
			return Task.FromResult( context.Value == null );
		}

		protected override string GetDefaultMessage( T value ) {
			return "Value must be null";
		}
	}
}