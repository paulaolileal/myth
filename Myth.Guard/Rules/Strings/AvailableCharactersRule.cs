using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Strings {

	internal sealed class AvailableCharactersRule : ValidationRuleBase<string> {
		private readonly char[ ] _allowedChars;

		public AvailableCharactersRule( char[ ] allowedChars ) {
			_allowedChars = allowedChars;
		}

		protected override Task<bool> EvaluateAsync( RuleContext<string> context ) {
			if ( string.IsNullOrEmpty( context.Value ) )
				return Task.FromResult( true );

			return Task.FromResult( context.Value.All( c => _allowedChars.Contains( c ) ) );
		}

		protected override string GetDefaultMessage( string value ) {
			return "Value contains characters that are not allowed";
		}
	}
}