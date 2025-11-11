using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Strings; 

internal sealed class StringEqualsRule : ValidationRuleBase<string> {
	private readonly string _expected;
	private readonly bool _ignoreCase;

	public StringEqualsRule( string expected, bool ignoreCase ) {
		_expected = expected;
		_ignoreCase = ignoreCase;
	}

	protected override Task<bool> EvaluateAsync( RuleContext<string> context ) {
		var comparison = _ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
		return Task.FromResult( string.Equals( context.Value, _expected, comparison ) );
	}

	protected override string GetDefaultMessage( string value ) {
		return $"Value must be equal to {_expected}";
	}
}
