using System.Text.RegularExpressions;
using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Strings;

internal sealed class EmailRule : ValidationRuleBase<string> {

	private static readonly Regex EmailRegex = new(
		@"^[^@\s]+@[^@\s]+\.[^@\s]+$",
		RegexOptions.Compiled | RegexOptions.IgnoreCase );

	protected override Task<bool> EvaluateAsync( RuleContext<string> context ) {
		if ( string.IsNullOrEmpty( context.Value ) )
			return Task.FromResult( false );

		return Task.FromResult( EmailRegex.IsMatch( context.Value ) );
	}

	protected override string GetDefaultMessage( string value ) {
		return "Invalid email format";
	}
}
