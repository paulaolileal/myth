using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Strings; 

internal sealed class NoSymbolsRule : ValidationRuleBase<string> {
	private readonly char[ ]? _symbols;

	public NoSymbolsRule( char[ ]? symbols ) {
		_symbols = symbols;
	}

	protected override Task<bool> EvaluateAsync( RuleContext<string> context ) {
		if ( string.IsNullOrEmpty( context.Value ) )
			return Task.FromResult( true );

		if ( _symbols == null || _symbols.Length == 0 )
			return Task.FromResult( context.Value.All( c => char.IsLetterOrDigit( c ) || char.IsWhiteSpace( c ) ) );

		return Task.FromResult( !context.Value.Any( c => _symbols.Contains( c ) ) );
	}

	protected override string GetDefaultMessage( string value ) {
		return "Value contains forbidden symbols";
	}
}
