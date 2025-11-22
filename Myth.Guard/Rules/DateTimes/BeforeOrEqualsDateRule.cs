using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.DateTimes;

internal sealed class BeforeOrEqualsDateRule( DateTime date ) : ValidationRuleBase<DateTime> {
	private readonly DateTime _date = date;

	protected override Task<bool> EvaluateAsync( RuleContext<DateTime> context ) {
		return Task.FromResult( context.Value <= _date );
	}

	protected override string GetDefaultMessage( DateTime value ) {
		return $"Date must be before or equal to {_date:yyyy-MM-dd}";
	}
}
