using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Strings; 

internal sealed class UrlRule : ValidationRuleBase<string> {

	protected override Task<bool> EvaluateAsync( RuleContext<string> context ) {
		if ( string.IsNullOrEmpty( context.Value ) )
			return Task.FromResult( false );

		return Task.FromResult( Uri.TryCreate( context.Value, UriKind.Absolute, out var uri ) &&
								new List<string> {
									Uri.UriSchemeFtps,
									Uri.UriSchemeFtp,
									Uri.UriSchemeHttp,
									Uri.UriSchemeHttps,
									Uri.UriSchemeMailto,
									Uri.UriSchemeNews,
									Uri.UriSchemeNntp,
									Uri.UriSchemeTelnet
								}.Contains( uri.Scheme ) );
	}

	protected override string GetDefaultMessage( string value ) {
		return "Invalid URL format";
	}
}
