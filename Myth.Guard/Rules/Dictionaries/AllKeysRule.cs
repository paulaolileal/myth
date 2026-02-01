using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Dictionaries;

/// <summary>
/// Validation rule that checks if all keys in a dictionary satisfy a predicate
/// </summary>
public sealed class AllKeysRule<TKey, TValue> : ValidationRuleBase<IDictionary<TKey, TValue>> {

	private readonly Func<TKey, bool> _predicate;

	public AllKeysRule( Func<TKey, bool> predicate ) {
		ArgumentNullException.ThrowIfNull( predicate, nameof( predicate ) );
		_predicate = predicate;
	}

	protected override Task<bool> EvaluateAsync( RuleContext<IDictionary<TKey, TValue>> context ) {
		if ( context.Value is null )
			return Task.FromResult( false );

		return Task.FromResult( context.Value.Keys.All( _predicate ) );
	}

	protected override string GetDefaultMessage( IDictionary<TKey, TValue> value ) {
		return "All dictionary keys must satisfy the specified condition";
	}
}
