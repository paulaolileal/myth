using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Dictionaries;

/// <summary>
/// Validation rule that checks if a dictionary count is greater than a minimum value
/// </summary>
public sealed class DictionaryCountGreaterThanRule<TKey, TValue> : ValidationRuleBase<IDictionary<TKey, TValue>> {

	private readonly int _min;

	public DictionaryCountGreaterThanRule( int min ) {
		if ( min < 0 )
			throw new ArgumentException( "Minimum count cannot be negative", nameof( min ) );

		_min = min;
	}

	protected override Task<bool> EvaluateAsync( RuleContext<IDictionary<TKey, TValue>> context ) {
		if ( context.Value is null )
			return Task.FromResult( false );

		return Task.FromResult( context.Value.Count > _min );
	}

	protected override string GetDefaultMessage( IDictionary<TKey, TValue> value ) {
		return $"Dictionary must have more than {_min} {( _min == 1 ? "entry" : "entries" )}";
	}
}
