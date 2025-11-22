using Myth.Interfaces.Rules;
using Myth.Rules.Base;

namespace Myth.Rules.Numerics;

/// <summary>
/// Numeric rule configurator
/// </summary>
public sealed class NumericRules<T>( FieldRuleBuilder<T> builder ) : RuleConfigurator<T>( builder ), INumericRules<T> where T : struct, IComparable<T> {
	public FieldRuleBuilder<T> GreaterThan( T min ) {
		Builder.AddRule( new GreaterThanRule<T>( min ) );
		return Builder;
	}

	public FieldRuleBuilder<T> GreaterOrEquals( T min ) {
		Builder.AddRule( new GreaterOrEqualsRule<T>( min ) );
		return Builder;
	}

	public FieldRuleBuilder<T> LessThan( T max ) {
		Builder.AddRule( new LessThanRule<T>( max ) );
		return Builder;
	}

	public FieldRuleBuilder<T> LessOrEquals( T max ) {
		Builder.AddRule( new LessOrEqualsRule<T>( max ) );
		return Builder;
	}

	public FieldRuleBuilder<T> Between( T min, T max ) {
		Builder.AddRule( new BetweenRule<T>( min, max ) );
		return Builder;
	}

	public FieldRuleBuilder<T> Positive( ) {
		Builder.AddRule( new PositiveRule<T>( ) );
		return Builder;
	}

	public FieldRuleBuilder<T> Negative( ) {
		Builder.AddRule( new NegativeRule<T>( ) );
		return Builder;
	}

	public FieldRuleBuilder<T> Zero( ) {
		Builder.AddRule( new ZeroRule<T>( ) );
		return Builder;
	}

	public FieldRuleBuilder<T> NotZero( ) {
		Builder.AddRule( new NotZeroRule<T>( ) );
		return Builder;
	}
}
