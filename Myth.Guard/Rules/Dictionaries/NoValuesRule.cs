using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Dictionaries;

/// <summary>
/// Validation rule that checks if no values in a dictionary satisfy a predicate
/// </summary>
public sealed class NoValuesRule<TKey, TValue> : ValidationRuleBase<IDictionary<TKey, TValue>> {

	private readonly Func<TValue, bool> _predicate;

	public NoValuesRule( Func<TValue, bool> predicate ) {
		ArgumentNullException.ThrowIfNull( predicate, nameof( predicate ) );
		_predicate = predicate;
	}

	protected override Task<bool> EvaluateAsync( RuleContext<IDictionary<TKey, TValue>> context ) {
		if ( context.Value is null )
			return Task.FromResult( true ); // No values in null dictionary

		return Task.FromResult( !context.Value.Values.Any( _predicate ) );
	}

	protected override string GetDefaultMessage( IDictionary<TKey, TValue> value ) {
		return "No dictionary values must satisfy the specified condition";
	}
}
