using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Strings {

	internal sealed class MinimumLengthRule : ValidationRuleBase<string> {
		private readonly int _minLength;

		public MinimumLengthRule( int minLength ) {
			_minLength = minLength;
		}

		protected override Task<bool> EvaluateAsync( RuleContext<string> context ) {
			return Task.FromResult( context.Value?.Length >= _minLength );
		}

		protected override string GetDefaultMessage( string value ) {
			return $"Minimum length is {_minLength}";
		}
	}
}