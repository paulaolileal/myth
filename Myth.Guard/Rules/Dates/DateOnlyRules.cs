using Myth.Interfaces.Rules;
using Myth.Rules.Base;

namespace Myth.Rules.Dates; 

/// <summary>
/// DateOnly rule configurator
/// </summary>
public sealed class DateOnlyRules : RuleConfigurator<DateOnly>, IDateOnlyRules {

	public DateOnlyRules( FieldRuleBuilder<DateOnly> builder ) : base( builder ) {
	}

	public FieldRuleBuilder<DateOnly> After( DateOnly date ) {
		Builder.AddRule( new AfterDateOnlyRule( date ) );
		return Builder;
	}

	public FieldRuleBuilder<DateOnly> AfterOrEquals( DateOnly date ) {
		Builder.AddRule( new AfterOrEqualsDateOnlyRule( date ) );
		return Builder;
	}

	public FieldRuleBuilder<DateOnly> Before( DateOnly date ) {
		Builder.AddRule( new BeforeDateOnlyRule( date ) );
		return Builder;
	}

	public FieldRuleBuilder<DateOnly> BeforeOrEquals( DateOnly date ) {
		Builder.AddRule( new BeforeOrEqualsDateOnlyRule( date ) );
		return Builder;
	}

	public FieldRuleBuilder<DateOnly> Between( DateOnly min, DateOnly max ) {
		Builder.AddRule( new BetweenDateOnlyRule( min, max ) );
		return Builder;
	}

	public FieldRuleBuilder<DateOnly> Today( ) {
		Builder.AddRule( new TodayDateOnlyRule( ) );
		return Builder;
	}

	public FieldRuleBuilder<DateOnly> Past( ) {
		Builder.AddRule( new PastDateOnlyRule( ) );
		return Builder;
	}

	public FieldRuleBuilder<DateOnly> Future( ) {
		Builder.AddRule( new FutureDateOnlyRule( ) );
		return Builder;
	}
}
