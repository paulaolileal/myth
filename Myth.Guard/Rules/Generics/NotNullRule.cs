using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Generics {

	internal sealed class NotNullRule<T> : ValidationRuleBase<T> {

		protected override Task<bool> EvaluateAsync( RuleContext<T> context ) {
			return Task.FromResult( context.Value != null );
		}

		protected override string GetDefaultMessage( T value ) {
			return "Value must not be null";
		}
	}
}