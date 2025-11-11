using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Strings; 

internal sealed class NotEmptyStringRule : ValidationRuleBase<string> {

	protected override Task<bool> EvaluateAsync( RuleContext<string> context ) {
		return Task.FromResult( !string.IsNullOrEmpty( context.Value ) );
	}

	protected override string GetDefaultMessage( string value ) {
		return "Value cannot be empty";
	}
}
