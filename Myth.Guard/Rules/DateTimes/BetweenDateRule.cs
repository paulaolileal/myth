using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.DateTimes;

internal sealed class BetweenDateRule( DateTime min, DateTime max ) : ValidationRuleBase<DateTime> {
	private readonly DateTime _min = min;
	private readonly DateTime _max = max;

	protected override Task<bool> EvaluateAsync( RuleContext<DateTime> context ) {
		return Task.FromResult( context.Value >= _min && context.Value <= _max );
	}

	protected override string GetDefaultMessage( DateTime value ) {
		return $"Date must be between {_min:yyyy-MM-dd} and {_max:yyyy-MM-dd}";
	}
}
