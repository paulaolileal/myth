using Myth.Interfaces.Rules;
using Myth.Rules.Base;

namespace Myth.Rules.Enums {

	/// <summary>
	/// Enum rule configurator
	/// </summary>
	public sealed class EnumRules<T> : RuleConfigurator<T>, IEnumRules<T> where T : struct, Enum {

		public EnumRules( FieldRuleBuilder<T> builder ) : base( builder ) {
		}

		public FieldRuleBuilder<T> BeInEnum( ) {
			Builder.AddRule( new BeInEnumRule<T>( ) );
			return Builder;
		}

		public FieldRuleBuilder<T> BeNotInEnum( ) {
			Builder.AddRule( new BeNotInEnumRule<T>( ) );
			return Builder;
		}

		public FieldRuleBuilder<T> Only( params T[ ] values ) {
			Builder.AddRule( new OnlyEnumValuesRule<T>( values ) );
			return Builder;
		}
	}
}