using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.DateTimes {

	internal sealed class BetweenDateRule : ValidationRuleBase<DateTime> {
		private readonly DateTime _min;
		private readonly DateTime _max;

		public BetweenDateRule( DateTime min, DateTime max ) {
			_min = min;
			_max = max;
		}

		protected override Task<bool> EvaluateAsync( RuleContext<DateTime> context ) {
			return Task.FromResult( context.Value >= _min && context.Value <= _max );
		}

		protected override string GetDefaultMessage( DateTime value ) {
			return $"Date must be between {_min:yyyy-MM-dd} and {_max:yyyy-MM-dd}";
		}
	}
}