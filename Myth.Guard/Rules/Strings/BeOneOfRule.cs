using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Strings {

	internal sealed class BeOneOfRule : ValidationRuleBase<string> {
		private readonly string[ ] _options;

		public BeOneOfRule( string[ ] options ) {
			_options = options;
		}

		protected override Task<bool> EvaluateAsync( RuleContext<string> context ) {
			return Task.FromResult( _options.Contains( context.Value ) );
		}

		protected override string GetDefaultMessage( string value ) {
			return $"Value must be one of: {string.Join( ", ", _options )}";
		}
	}
}