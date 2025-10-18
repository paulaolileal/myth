using Myth.Rules.Base;

namespace Myth.Interfaces.Rules {

	/// <summary>
	/// Enum-specific validation rules
	/// </summary>
	public interface IEnumRules<T> : IRuleConfigurator<T> where T : struct, Enum {

		FieldRuleBuilder<T> BeInEnum( );

		FieldRuleBuilder<T> BeNotInEnum( );

		FieldRuleBuilder<T> Only( params T[ ] values );
	}
}