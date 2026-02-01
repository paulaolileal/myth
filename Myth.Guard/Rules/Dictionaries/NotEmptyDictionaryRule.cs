using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Dictionaries;

/// <summary>
/// Validation rule that checks if a dictionary is not null and not empty
/// </summary>
public sealed class NotEmptyDictionaryRule<TKey, TValue> : ValidationRuleBase<IDictionary<TKey, TValue>> {

	protected override Task<bool> EvaluateAsync( RuleContext<IDictionary<TKey, TValue>> context ) {
		return Task.FromResult( context.Value is not null && context.Value.Count > 0 );
	}

	protected override string GetDefaultMessage( IDictionary<TKey, TValue> value ) {
		return "Dictionary cannot be empty";
	}
}
