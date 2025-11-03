using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Dates {

	internal sealed class AfterOrEqualsDateOnlyRule : ValidationRuleBase<DateOnly> {
		private readonly DateOnly _date;

		public AfterOrEqualsDateOnlyRule( DateOnly date ) {
			_date = date;
		}

		protected override Task<bool> EvaluateAsync( RuleContext<DateOnly> context ) {
			return Task.FromResult( context.Value >= _date );
		}

		protected override string GetDefaultMessage( DateOnly value ) {
			return $"Date must be after or equal to {_date:yyyy-MM-dd}";
		}
	}
}