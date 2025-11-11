using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Strings;

internal sealed class ForbiddenCharactersRule : ValidationRuleBase<string> {
	private readonly char[ ] _forbiddenChars;

	public ForbiddenCharactersRule( char[ ] forbiddenChars ) {
		_forbiddenChars = forbiddenChars;
	}

	protected override Task<bool> EvaluateAsync( RuleContext<string> context ) {
		if ( string.IsNullOrEmpty( context.Value ) )
			return Task.FromResult( true );

		return Task.FromResult( !context.Value.Any( c => _forbiddenChars.Contains( c ) ) );
	}

	protected override string GetDefaultMessage( string value ) {
		return "Value contains forbidden characters";
	}
}
