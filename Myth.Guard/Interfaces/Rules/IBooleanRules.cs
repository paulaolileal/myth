using Myth.Rules.Base;

namespace Myth.Interfaces.Rules {

	/// <summary>
	/// Boolean-specific validation rules
	/// </summary>
	public interface IBooleanRules : IRuleConfigurator<bool> {

		FieldRuleBuilder<bool> IsTrue( );

		FieldRuleBuilder<bool> IsFalse( );

		FieldRuleBuilder<bool> Not( );
	}
}