using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Guard.Rules.Nullables.Numerics;

internal sealed class NullableZeroRule<T> : ValidationRuleBase<T?>
	where T : struct, IComparable<T> {
	private static readonly T Zero = ( T )Convert.ChangeType( 0, typeof( T ) );

	protected override Task<bool> EvaluateAsync( RuleContext<T?> context ) {
		if ( !context.Value.HasValue )
			return Task.FromResult( true );

		return Task.FromResult( context.Value.Value.CompareTo( Zero ) == 0 );
	}

	protected override string GetDefaultMessage( T? value ) {
		return value.HasValue
			? $"The value '{value.Value}' must be zero."
			: "The value must be zero.";
	}
}
