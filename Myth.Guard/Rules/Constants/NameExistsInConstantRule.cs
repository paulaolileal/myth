using Myth.Enums;
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
			return Task.FromResult( Constant<TConstant, TValue>.TryFromName( context.Value, out _ ) );
		} catch ( InvalidOperationException ) {
			return Task.FromResult( false );
		}
	}

	protected override string GetDefaultMessage( string value ) {
		return $"Name '{value}' is not valid. Valid options are: {Constant<TConstant, TValue>.GetOptions( )}";
	}

	protected override IReadOnlyList<string>? GetOptionsForError( string value ) {
		if ( !HasOptionsConfigured )
			return null;

		if ( Options != null )
			return Options;

		// Generate options automatically from constant names/values
		try {
			var constantValues = Constant<TConstant, TValue>.GetAll( );
			return constantValues.Select( c => FormatConstantOption( c.Name, c.Value, OptionsType ) ).ToList( ).AsReadOnly( );
		} catch ( InvalidOperationException ) {
			return null;
		}
	}

	private string FormatConstantOption( string name, TValue value, OptionsType type ) {
		return type switch {
			OptionsType.OnlyValue => value?.ToString( ) ?? string.Empty,
			OptionsType.OnlyName => name,
			OptionsType.ValueAndName => $"{value}: {name}",
			_ => $"{value}: {name}"
		};
	}
}
