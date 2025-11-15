using Myth.Interfaces;

namespace Myth.Rules.Base;

/// <summary>
/// Rule builder for a specific field
/// </summary>
/// <typeparam name="T">The field value type</typeparam>
public sealed class FieldRuleBuilder<T> {
	private readonly List<IValidationRule<T>> _rules = new( );
	internal string FieldName { get; }

	internal FieldRuleBuilder( string fieldName ) {
		FieldName = fieldName;
	}

	internal void AddRule( IValidationRule<T> rule ) {
		_rules.Add( rule );
	}

	internal IReadOnlyList<IValidationRule<T>> GetRules( ) => _rules;
}
