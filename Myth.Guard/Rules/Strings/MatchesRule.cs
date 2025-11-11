using System.Text.RegularExpressions;
using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Strings; 

internal sealed class MatchesRule : ValidationRuleBase<string> {
	private readonly Regex _regex;

	public MatchesRule( Regex regex ) {
		_regex = regex;
	}

	protected override Task<bool> EvaluateAsync( RuleContext<string> context ) {
		if ( string.IsNullOrEmpty( context.Value ) )
			return Task.FromResult( false );

		return Task.FromResult( _regex.IsMatch( context.Value ) );
	}

	protected override string GetDefaultMessage( string value ) {
		return "Value does not match the required pattern";
	}
}
