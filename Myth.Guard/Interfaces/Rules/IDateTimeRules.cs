using Myth.Rules.Base;

namespace Myth.Interfaces.Rules {

	/// <summary>
	/// DateTime-specific validation rules
	/// </summary>
	public interface IDateTimeRules : IRuleConfigurator<DateTime> {

		FieldRuleBuilder<DateTime> After( DateTime date );

		FieldRuleBuilder<DateTime> AfterOrEquals( DateTime date );

		FieldRuleBuilder<DateTime> Before( DateTime date );

		FieldRuleBuilder<DateTime> BeforeOrEquals( DateTime date );

		FieldRuleBuilder<DateTime> Between( DateTime min, DateTime max );

		FieldRuleBuilder<DateTime> Today( );

		FieldRuleBuilder<DateTime> Past( );

		FieldRuleBuilder<DateTime> Future( );
	}
}