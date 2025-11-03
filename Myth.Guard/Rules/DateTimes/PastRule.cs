using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.DateTimes {

	internal sealed class PastRule : ValidationRuleBase<DateTime> {

		protected override Task<bool> EvaluateAsync( RuleContext<DateTime> context ) {
			return Task.FromResult( context.Value < DateTime.Now );
		}

		protected override string GetDefaultMessage( DateTime value ) {
			return "Date must be in the past";
		}
	}
}