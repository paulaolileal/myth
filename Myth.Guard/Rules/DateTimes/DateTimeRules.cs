using Myth.Interfaces.Rules;
using Myth.Rules.Base;

namespace Myth.Rules.DateTimes;

/// <summary>
/// DateTime rule configurator
/// </summary>
public sealed class DateTimeRules : RuleConfigurator<DateTime>, IDateTimeRules {

	public DateTimeRules( FieldRuleBuilder<DateTime> builder ) : base( builder ) {
	}

	public FieldRuleBuilder<DateTime> After( DateTime date ) {
		Builder.AddRule( new AfterDateRule( date ) );
		return Builder;
	}

	public FieldRuleBuilder<DateTime> AfterOrEquals( DateTime date ) {
		Builder.AddRule( new AfterOrEqualsDateRule( date ) );
		return Builder;
	}

	public FieldRuleBuilder<DateTime> Before( DateTime date ) {
		Builder.AddRule( new BeforeDateRule( date ) );
		return Builder;
	}

	public FieldRuleBuilder<DateTime> BeforeOrEquals( DateTime date ) {
		Builder.AddRule( new BeforeOrEqualsDateRule( date ) );
		return Builder;
	}

	public FieldRuleBuilder<DateTime> Between( DateTime min, DateTime max ) {
		Builder.AddRule( new BetweenDateRule( min, max ) );
		return Builder;
	}

	public FieldRuleBuilder<DateTime> Today( ) {
		Builder.AddRule( new TodayRule( ) );
		return Builder;
	}

	public FieldRuleBuilder<DateTime> Past( ) {
		Builder.AddRule( new PastRule( ) );
		return Builder;
	}

	public FieldRuleBuilder<DateTime> Future( ) {
		Builder.AddRule( new FutureRule( ) );
		return Builder;
	}
}
