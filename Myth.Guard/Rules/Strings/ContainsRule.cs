using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Strings; 

internal sealed class ContainsRule : ValidationRuleBase<string> {
	private readonly string _substring;
	private readonly bool _ignoreCase;

	public ContainsRule( string substring, bool ignoreCase ) {
		_substring = substring;
		_ignoreCase = ignoreCase;
	}

	protected override Task<bool> EvaluateAsync( RuleContext<string> context ) {
		if ( string.IsNullOrEmpty( context.Value ) )
			return Task.FromResult( false );

		var comparison = _ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
		return Task.FromResult( context.Value.Contains( _substring, comparison ) );
	}

	protected override string GetDefaultMessage( string value ) {
		return $"Value must contain {_substring}";
	}
}
