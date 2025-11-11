using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Enums; 

internal sealed class OnlyEnumValuesRule<T> : ValidationRuleBase<T> where T : struct, Enum {
	private readonly T[ ] _allowedValues;

	public OnlyEnumValuesRule( T[ ] allowedValues ) {
		_allowedValues = allowedValues;
	}

	protected override Task<bool> EvaluateAsync( RuleContext<T> context ) {
		return Task.FromResult( _allowedValues.Contains( context.Value ) );
	}

	protected override string GetDefaultMessage( T value ) {
		return $"Value must be one of: {string.Join( ", ", _allowedValues )}";
	}
}
