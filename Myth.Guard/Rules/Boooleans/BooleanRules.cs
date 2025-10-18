using Myth.Interfaces.Rules;
using Myth.Rules.Base;

namespace Myth.Rules.Boooleans {

	/// <summary>
	/// Boolean rule configurator
	/// </summary>
	public sealed class BooleanRules : RuleConfigurator<bool>, IBooleanRules {

		public BooleanRules( FieldRuleBuilder<bool> builder ) : base( builder ) {
		}

		public FieldRuleBuilder<bool> IsTrue( ) {
			Builder.AddRule( new IsTrueRule( ) );
			return Builder;
		}

		public FieldRuleBuilder<bool> IsFalse( ) {
			Builder.AddRule( new IsFalseRule( ) );
			return Builder;
		}

		public FieldRuleBuilder<bool> Not( ) {
			Builder.AddRule( new IsFalseRule( ) );
			return Builder;
		}
	}
}