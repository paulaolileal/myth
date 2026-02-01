using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Dictionaries;

/// <summary>
/// Validation rule that checks if any key in a dictionary satisfies a predicate
/// </summary>
public sealed class AnyKeyRule<TKey, TValue> : ValidationRuleBase<IDictionary<TKey, TValue>> {

	private readonly Func<TKey, bool> _predicate;

	public AnyKeyRule( Func<TKey, bool> predicate ) {
		ArgumentNullException.ThrowIfNull( predicate, nameof( predicate ) );
		_predicate = predicate;
	}

	protected override Task<bool> EvaluateAsync( RuleContext<IDictionary<TKey, TValue>> context ) {
		if ( context.Value is null || context.Value.Count == 0 )
			return Task.FromResult( false );

		return Task.FromResult( context.Value.Keys.Any( _predicate ) );
	}

	protected override string GetDefaultMessage( IDictionary<TKey, TValue> value ) {
		return "At least one dictionary key must satisfy the specified condition";
	}
}
