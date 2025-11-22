using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Guard.Rules.Nullables.Numerics;

internal sealed class NullableBetweenRule<T>( T min, T max ) : ValidationRuleBase<T?>
	where T : struct, IComparable<T> {
	private readonly T _min = min;
	private readonly T _max = max;

	protected override Task<bool> EvaluateAsync( RuleContext<T?> context ) {
		if ( !context.Value.HasValue )
			return Task.FromResult( true );

		var value = context.Value.Value;
		return Task.FromResult( value.CompareTo( _min ) >= 0 && value.CompareTo( _max ) <= 0 );
	}

	protected override string GetDefaultMessage( T? value ) {
		return value.HasValue
			? $"The value '{value.Value}' must be between '{_min}' and '{_max}'."
			: $"The value must be between '{_min}' and '{_max}'.";
	}
}
