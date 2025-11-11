using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Generics; 

internal sealed class NotEqualsRule<T> : ValidationRuleBase<T> {
	private readonly T _notExpected;

	public NotEqualsRule( T notExpected ) {
		_notExpected = notExpected;
	}

	protected override Task<bool> EvaluateAsync( RuleContext<T> context ) {
		return Task.FromResult( !EqualityComparer<T>.Default.Equals( context.Value, _notExpected ) );
	}

	protected override string GetDefaultMessage( T value ) {
		return $"Value must not be equal to {_notExpected}";
	}
}
