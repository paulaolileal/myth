using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Strings; 

internal sealed class StartsWithRule : ValidationRuleBase<string> {
	private readonly string _prefix;
	private readonly bool _ignoreCase;

	public StartsWithRule( string prefix, bool ignoreCase ) {
		_prefix = prefix;
		_ignoreCase = ignoreCase;
	}

	protected override Task<bool> EvaluateAsync( RuleContext<string> context ) {
		if ( string.IsNullOrEmpty( context.Value ) )
			return Task.FromResult( false );

		var comparison = _ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
		return Task.FromResult( context.Value.StartsWith( _prefix, comparison ) );
	}

	protected override string GetDefaultMessage( string value ) {
		return $"Value must start with {_prefix}";
	}
}
