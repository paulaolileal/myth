using Myth.Rules.Base;

namespace Myth.Interfaces.Rules {

	/// <summary>
	/// Generic validation rules applicable to any type
	/// </summary>
	public interface IGenericRules<T> {

		FieldRuleBuilder<T> BeDefault( );

		FieldRuleBuilder<T> NotDefault( );
	}
}