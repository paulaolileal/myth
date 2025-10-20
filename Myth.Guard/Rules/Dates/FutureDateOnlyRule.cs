using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Dates {

	internal sealed class FutureDateOnlyRule : ValidationRuleBase<DateOnly> {

		protected override Task<bool> EvaluateAsync( RuleContext<DateOnly> context ) {
			return Task.FromResult( context.Value > DateOnly.FromDateTime( DateTime.Today ) );
		}

		protected override string GetDefaultMessage( DateOnly value ) {
			return "Date must be in the future";
		}
	}
}