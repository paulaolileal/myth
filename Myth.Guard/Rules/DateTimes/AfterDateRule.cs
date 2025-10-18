using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.DateTimes {

	internal sealed class AfterDateRule : ValidationRuleBase<DateTime> {
		private readonly DateTime _date;

		public AfterDateRule( DateTime date ) {
			_date = date;
		}

		protected override Task<bool> EvaluateAsync( RuleContext<DateTime> context ) {
			return Task.FromResult( context.Value > _date );
		}

		protected override string GetDefaultMessage( DateTime value ) {
			return $"Date must be after {_date:yyyy-MM-dd}";
		}
	}
}