using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Dictionaries;

/// <summary>
/// Validation rule that checks if a dictionary contains a specific value
/// </summary>
public sealed class ContainsValueRule<TKey, TValue>( TValue value ) : ValidationRuleBase<IDictionary<TKey, TValue>> {

	private readonly TValue _value = value;

	protected override Task<bool> EvaluateAsync( RuleContext<IDictionary<TKey, TValue>> context ) {
		if ( context.Value is null )
			return Task.FromResult( false );

		return Task.FromResult( context.Value.Values.Contains( _value ) );
	}

	protected override string GetDefaultMessage( IDictionary<TKey, TValue> value ) {
		return $"Dictionary must contain value '{_value}'";
	}
}
