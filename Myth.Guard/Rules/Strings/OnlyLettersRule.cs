using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Strings;

internal sealed class OnlyLettersRule : ValidationRuleBase<string> {

	protected override Task<bool> EvaluateAsync( RuleContext<string> context ) {
		if ( string.IsNullOrEmpty( context.Value ) )
			return Task.FromResult( false );

		return Task.FromResult( context.Value.All( char.IsLetter ) );
	}

	protected override string GetDefaultMessage( string value ) {
		return "Value must contain only letters";
	}
}
