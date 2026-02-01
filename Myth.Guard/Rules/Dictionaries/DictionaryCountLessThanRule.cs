using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Dictionaries;

/// <summary>
/// Validation rule that checks if a dictionary count is less than a maximum value
/// </summary>
public sealed class DictionaryCountLessThanRule<TKey, TValue> : ValidationRuleBase<IDictionary<TKey, TValue>> {

	private readonly int _max;

	public DictionaryCountLessThanRule( int max ) {
		if ( max < 0 )
			throw new ArgumentException( "Maximum count cannot be negative", nameof( max ) );

		_max = max;
	}

	protected override Task<bool> EvaluateAsync( RuleContext<IDictionary<TKey, TValue>> context ) {
		if ( context.Value is null )
			return Task.FromResult( false );

		return Task.FromResult( context.Value.Count < _max );
	}

	protected override string GetDefaultMessage( IDictionary<TKey, TValue> value ) {
		return $"Dictionary must have fewer than {_max} {( _max == 1 ? "entry" : "entries" )}";
	}
}
