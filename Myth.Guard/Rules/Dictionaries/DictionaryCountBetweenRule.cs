using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Dictionaries;

/// <summary>
/// Validation rule that checks if a dictionary count is between minimum and maximum values (inclusive)
/// </summary>
public sealed class DictionaryCountBetweenRule<TKey, TValue> : ValidationRuleBase<IDictionary<TKey, TValue>> {

	private readonly int _min;
	private readonly int _max;

	public DictionaryCountBetweenRule( int min, int max ) {
		if ( min < 0 )
			throw new ArgumentException( "Minimum count cannot be negative", nameof( min ) );

		if ( max < 0 )
			throw new ArgumentException( "Maximum count cannot be negative", nameof( max ) );

		if ( min > max )
			throw new ArgumentException( "Minimum count cannot be greater than maximum count", nameof( min ) );

		_min = min;
		_max = max;
	}

	protected override Task<bool> EvaluateAsync( RuleContext<IDictionary<TKey, TValue>> context ) {
		if ( context.Value is null )
			return Task.FromResult( false );

		var count = context.Value.Count;
		return Task.FromResult( count >= _min && count <= _max );
	}

	protected override string GetDefaultMessage( IDictionary<TKey, TValue> value ) {
		return $"Dictionary must have between {_min} and {_max} {( _max == 1 ? "entry" : "entries" )}";
	}
}
