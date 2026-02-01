using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Dictionaries;

/// <summary>
/// Validation rule that checks if a dictionary does not contain a specific key
/// </summary>
public sealed class NotContainsKeyRule<TKey, TValue> : ValidationRuleBase<IDictionary<TKey, TValue>> {

	private readonly TKey _key;

	public NotContainsKeyRule( TKey key ) {
		ArgumentNullException.ThrowIfNull( key, nameof( key ) );
		_key = key;
	}

	protected override Task<bool> EvaluateAsync( RuleContext<IDictionary<TKey, TValue>> context ) {
		if ( context.Value is null )
			return Task.FromResult( true ); // Null dictionary doesn't contain the key

		return Task.FromResult( !context.Value.ContainsKey( _key ) );
	}

	protected override string GetDefaultMessage( IDictionary<TKey, TValue> value ) {
		return $"Dictionary must not contain key '{_key}'";
	}
}
