using Ardalis.SmartEnum;
using Myth.Models;
using Myth.Rules.Base;
using Myth.ValueObjects;

namespace Myth.Rules.Constants;

/// <summary>
/// Validates that a name exists within a Constant type definition
/// </summary>
/// <typeparam name="TConstant">The constant type</typeparam>
/// <typeparam name="TValue">The value type</typeparam>
internal sealed class NameExistsInConstantRule<TConstant, TValue> : ValidationRuleBase<string>
	where TConstant : Constant<TConstant, TValue>
	where TValue : IEquatable<TValue>, IComparable<TValue> {

	protected override Task<bool> EvaluateAsync( RuleContext<string> context ) {
		if ( string.IsNullOrWhiteSpace( context.Value ) )
			return Task.FromResult( true ); // Null/empty values are handled by NotEmpty rule if needed

		try {
			var constant = SmartEnum<TConstant, TValue>.FromName( context.Value );
			return Task.FromResult( constant != null );
		} catch ( SmartEnumNotFoundException ) {
			return Task.FromResult( false );
		} catch ( InvalidOperationException ) {
			return Task.FromResult( false );
		}
	}

	protected override string GetDefaultMessage( string value ) {
		return $"Name '{value}' is not valid. Valid options are: {Constant<TConstant, TValue>.GetOptions( )}";
	}
}
