using System.Text.RegularExpressions;
using Myth.Interfaces.Rules;
using Myth.Rules.Base;

namespace Myth.Rules.Strings; 

/// <summary>
/// String rule configurator
/// </summary>
public sealed class StringRules : RuleConfigurator<string>, IStringRules {

	public StringRules( FieldRuleBuilder<string> builder ) : base( builder ) {
	}

	public FieldRuleBuilder<string> NotEmpty( ) {
		Builder.AddRule( new NotEmptyStringRule( ) );
		return Builder;
	}

	public FieldRuleBuilder<string> MinimumLength( int length ) {
		Builder.AddRule( new MinimumLengthRule( length ) );
		return Builder;
	}

	public FieldRuleBuilder<string> MaximumLength( int length ) {
		Builder.AddRule( new MaximumLengthRule( length ) );
		return Builder;
	}

	public FieldRuleBuilder<string> LengthBetween( int min, int max ) {
		Builder.AddRule( new LengthBetweenRule( min, max ) );
		return Builder;
	}

	public FieldRuleBuilder<string> EqualsTo( string expected, bool ignoreCase = false ) {
		Builder.AddRule( new StringEqualsRule( expected, ignoreCase ) );
		return Builder;
	}

	public FieldRuleBuilder<string> StartsWith( string prefix, bool ignoreCase = false ) {
		Builder.AddRule( new StartsWithRule( prefix, ignoreCase ) );
		return Builder;
	}

	public FieldRuleBuilder<string> EndsWith( string suffix, bool ignoreCase = false ) {
		Builder.AddRule( new EndsWithRule( suffix, ignoreCase ) );
		return Builder;
	}

	public FieldRuleBuilder<string> Contains( string substring, bool ignoreCase = false ) {
		Builder.AddRule( new ContainsRule( substring, ignoreCase ) );
		return Builder;
	}

	public FieldRuleBuilder<string> Matches( Regex regex ) {
		Builder.AddRule( new MatchesRule( regex ) );
		return Builder;
	}

	public FieldRuleBuilder<string> Email( ) {
		Builder.AddRule( new EmailRule( ) );
		return Builder;
	}

	public FieldRuleBuilder<string> Url( ) {
		Builder.AddRule( new UrlRule( ) );
		return Builder;
	}

	public FieldRuleBuilder<string> OnlyLetters( ) {
		Builder.AddRule( new OnlyLettersRule( ) );
		return Builder;
	}

	public FieldRuleBuilder<string> OnlyNumbers( ) {
		Builder.AddRule( new OnlyNumbersRule( ) );
		return Builder;
	}

	public FieldRuleBuilder<string> Alphanumeric( ) {
		Builder.AddRule( new AlphanumericRule( ) );
		return Builder;
	}

	public FieldRuleBuilder<string> NoSymbols( char[ ]? symbols = null ) {
		Builder.AddRule( new NoSymbolsRule( symbols ) );
		return Builder;
	}

	public FieldRuleBuilder<string> AvailableCharacters( params char[ ] chars ) {
		Builder.AddRule( new AvailableCharactersRule( chars ) );
		return Builder;
	}

	public FieldRuleBuilder<string> ForbiddenCharacters( params char[ ] chars ) {
		Builder.AddRule( new ForbiddenCharactersRule( chars ) );
		return Builder;
	}

	public FieldRuleBuilder<string> BeOneOf( params string[ ] options ) {
		Builder.AddRule( new BeOneOfRule( options ) );
		return Builder;
	}
}
