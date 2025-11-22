using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Strings;

internal sealed class MaximumLengthRule( int maxLength ) : ValidationRuleBase<string> {
	private readonly int _maxLength = maxLength;

	protected override Task<bool> EvaluateAsync( RuleContext<string> context ) {
		return Task.FromResult( context.Value?.Length <= _maxLength );
	}

	protected override string GetDefaultMessage( string value ) {
		return $"Maximum length is {_maxLength}";
	}
}
