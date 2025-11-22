using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Dates;

internal sealed class BetweenDateOnlyRule( DateOnly min, DateOnly max ) : ValidationRuleBase<DateOnly> {
	private readonly DateOnly _min = min;
	private readonly DateOnly _max = max;

	protected override Task<bool> EvaluateAsync( RuleContext<DateOnly> context ) {
		return Task.FromResult( context.Value >= _min && context.Value <= _max );
	}

	protected override string GetDefaultMessage( DateOnly value ) {
		return $"Date must be between {_min:yyyy-MM-dd} and {_max:yyyy-MM-dd}";
	}
}
