using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Dictionaries;

/// <summary>
/// Validation rule that checks if a dictionary contains a specific key
/// </summary>
public sealed class ContainsKeyRule<TKey, TValue> : ValidationRuleBase<IDictionary<TKey, TValue>> {

	private readonly TKey _key;

	public ContainsKeyRule( TKey key ) {
		ArgumentNullException.ThrowIfNull( key, nameof( key ) );
		_key = key;
	}

	protected override Task<bool> EvaluateAsync( RuleContext<IDictionary<TKey, TValue>> context ) {
		if ( context.Value is null )
			return Task.FromResult( false );

		return Task.FromResult( context.Value.ContainsKey( _key ) );
	}

	protected override string GetDefaultMessage( IDictionary<TKey, TValue> value ) {
		return $"Dictionary must contain key '{_key}'";
	}
}
