using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Generics;

internal sealed class EqualsRule<T> : ValidationRuleBase<T> {
	private readonly T _expected;

	public EqualsRule( T expected ) {
		_expected = expected;
	}

	protected override Task<bool> EvaluateAsync( RuleContext<T> context ) {
		return Task.FromResult( EqualityComparer<T>.Default.Equals( context.Value, _expected ) );
	}

	protected override string GetDefaultMessage( T value ) {
		return $"Value must be equal to {_expected}";
	}
}
