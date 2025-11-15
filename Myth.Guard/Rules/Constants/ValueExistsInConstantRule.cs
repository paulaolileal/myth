using Myth.Models;
using Myth.Rules.Base;
using Myth.ValueObjects;

namespace Myth.Rules.Constants;

/// <summary>
/// Validates that a value exists within a Constant type definition
/// </summary>
/// <typeparam name="TConstant">The constant type</typeparam>
/// <typeparam name="TValue">The value type</typeparam>
internal sealed class ValueExistsInConstantRule<TConstant, TValue> : ValidationRuleBase<TValue>
	where TConstant : Constant<TConstant, TValue>
	where TValue : IEquatable<TValue>, IComparable<TValue> {

	protected override Task<bool> EvaluateAsync( RuleContext<TValue> context ) {
		if ( context.Value == null )
			return Task.FromResult( true ); // Null values are handled by NotNull rule if needed

		try {
			return Task.FromResult( Constant<TConstant, TValue>.TryFromValue( context.Value, out _ ) );
		} catch ( InvalidOperationException ) {
			return Task.FromResult( false );
		}
	}

	protected override string GetDefaultMessage( TValue value ) {
		return $"Value '{value}' is not valid. Valid options are: {Constant<TConstant, TValue>.GetOptions( )}";
	}
}
