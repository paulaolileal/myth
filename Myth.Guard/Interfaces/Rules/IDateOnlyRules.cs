using Myth.Rules.Base;

namespace Myth.Interfaces.Rules; 

/// <summary>
/// DateOnly-specific validation rules
/// </summary>
public interface IDateOnlyRules : IRuleConfigurator<DateOnly> {

	FieldRuleBuilder<DateOnly> After( DateOnly date );

	FieldRuleBuilder<DateOnly> AfterOrEquals( DateOnly date );

	FieldRuleBuilder<DateOnly> Before( DateOnly date );

	FieldRuleBuilder<DateOnly> BeforeOrEquals( DateOnly date );

	FieldRuleBuilder<DateOnly> Between( DateOnly min, DateOnly max );

	FieldRuleBuilder<DateOnly> Today( );

	FieldRuleBuilder<DateOnly> Past( );

	FieldRuleBuilder<DateOnly> Future( );
}
