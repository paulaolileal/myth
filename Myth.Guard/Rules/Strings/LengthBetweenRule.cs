using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Strings;

internal sealed class LengthBetweenRule( int min, int max ) : ValidationRuleBase<string> {
	private readonly int _min = min;
	private readonly int _max = max;

	protected override Task<bool> EvaluateAsync( RuleContext<string> context ) {
		var length = context.Value?.Length ?? 0;
		return Task.FromResult( length >= _min && length <= _max );
	}

	protected override string GetDefaultMessage( string value ) {
		return $"Length must be between {_min} and {_max}";
	}
}
