using Myth.Interfaces.Rules;
using Myth.Rules.Base;

namespace Myth.Rules.Generics; 

/// <summary>
/// Generic rule configurator with default implementation
/// </summary>
public class GenericRules<T> : RuleConfigurator<T>, IGenericRules<T> {

	public GenericRules( FieldRuleBuilder<T> builder ) : base( builder ) {
	}

	public FieldRuleBuilder<T> BeDefault( ) {
		Builder.AddRule( new BeDefaultRule<T>( ) );
		return Builder;
	}

	public FieldRuleBuilder<T> NotDefault( ) {
		Builder.AddRule( new NotDefaultRule<T>( ) );
		return Builder;
	}
}
