using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Strings {

	internal sealed class EndsWithRule : ValidationRuleBase<string> {
		private readonly string _suffix;
		private readonly bool _ignoreCase;

		public EndsWithRule( string suffix, bool ignoreCase ) {
			_suffix = suffix;
			_ignoreCase = ignoreCase;
		}

		protected override Task<bool> EvaluateAsync( RuleContext<string> context ) {
			if ( string.IsNullOrEmpty( context.Value ) )
				return Task.FromResult( false );

			var comparison = _ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
			return Task.FromResult( context.Value.EndsWith( _suffix, comparison ) );
		}

		protected override string GetDefaultMessage( string value ) {
			return $"Value must end with {_suffix}";
		}
	}
}