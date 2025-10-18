using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Strings {

	internal sealed class UrlRule : ValidationRuleBase<string> {

		protected override Task<bool> EvaluateAsync( RuleContext<string> context ) {
			if ( string.IsNullOrEmpty( context.Value ) )
				return Task.FromResult( false );

			return Task.FromResult( Uri.TryCreate( context.Value, UriKind.Absolute, out var uri ) &&
								   ( uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps ) );
		}

		protected override string GetDefaultMessage( string value ) {
			return "Invalid URL format";
		}
	}
}