using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Guard.Rules.Nullables.Numerics;

internal sealed class NullableLessOrEqualsRule<T>( T compareValue ) : ValidationRuleBase<T?>
	where T : struct, IComparable<T> {
	private readonly T _compareValue = compareValue;

	protected override Task<bool> EvaluateAsync( RuleContext<T?> context ) {
		if ( !context.Value.HasValue )
			return Task.FromResult( true );

		return Task.FromResult( context.Value.Value.CompareTo( _compareValue ) <= 0 );
	}

	protected override string GetDefaultMessage( T? value ) {
		return value.HasValue
			? $"The value '{value.Value}' must be less than or equal to '{_compareValue}'."
			: $"The value must be less than or equal to '{_compareValue}'.";
	}
}
