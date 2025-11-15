using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.DateTimes;

internal sealed class TodayRule : ValidationRuleBase<DateTime> {

	protected override Task<bool> EvaluateAsync( RuleContext<DateTime> context ) {
		return Task.FromResult( context.Value.Date == DateTime.Today );
	}

	protected override string GetDefaultMessage( DateTime value ) {
		return "Date must be today";
	}
}
