using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Boooleans {

	internal sealed class IsTrueRule : ValidationRuleBase<bool> {

		protected override Task<bool> EvaluateAsync( RuleContext<bool> context ) {
			return Task.FromResult( context.Value );
		}

		protected override string GetDefaultMessage( bool value ) {
			return "Value must be true";
		}
	}
}