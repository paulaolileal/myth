using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Dictionaries;

/// <summary>
/// Validation rule that checks if any value in a dictionary satisfies a predicate
/// </summary>
public sealed class AnyValueRule<TKey, TValue> : ValidationRuleBase<IDictionary<TKey, TValue>> {

	private readonly Func<TValue, bool> _predicate;

	public AnyValueRule( Func<TValue, bool> predicate ) {
		ArgumentNullException.ThrowIfNull( predicate, nameof( predicate ) );
		_predicate = predicate;
	}

	protected override Task<bool> EvaluateAsync( RuleContext<IDictionary<TKey, TValue>> context ) {
		if ( context.Value is null || context.Value.Count == 0 )
			return Task.FromResult( false );

		return Task.FromResult( context.Value.Values.Any( _predicate ) );
	}

	protected override string GetDefaultMessage( IDictionary<TKey, TValue> value ) {
		return "At least one dictionary value must satisfy the specified condition";
	}
}
