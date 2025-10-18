using Myth.Rules.Base;

namespace Myth.Interfaces.Rules {

	/// <summary>
	/// Numeric-specific validation rules
	/// </summary>
	public interface INumericRules<T> : IRuleConfigurator<T> where T : struct, IComparable<T> {

		FieldRuleBuilder<T> GreaterThan( T min );

		FieldRuleBuilder<T> GreaterOrEquals( T min );

		FieldRuleBuilder<T> LessThan( T max );

		FieldRuleBuilder<T> LessOrEquals( T max );

		FieldRuleBuilder<T> Between( T min, T max );

		FieldRuleBuilder<T> Positive( );

		FieldRuleBuilder<T> Negative( );

		FieldRuleBuilder<T> Zero( );

		FieldRuleBuilder<T> NotZero( );
	}
}