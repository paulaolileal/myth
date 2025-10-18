using Myth.Rules.Base;
using System.Text.RegularExpressions;

namespace Myth.Interfaces.Rules {

	/// <summary>
	/// String-specific validation rules
	/// </summary>
	public interface IStringRules : IRuleConfigurator<string> {

		FieldRuleBuilder<string> NotEmpty( );

		FieldRuleBuilder<string> MinimumLength( int length );

		FieldRuleBuilder<string> MaximumLength( int length );

		FieldRuleBuilder<string> LengthBetween( int min, int max );

		FieldRuleBuilder<string> EqualsTo( string expected, bool ignoreCase = false );

		FieldRuleBuilder<string> StartsWith( string prefix, bool ignoreCase = false );

		FieldRuleBuilder<string> EndsWith( string suffix, bool ignoreCase = false );

		FieldRuleBuilder<string> Contains( string substring, bool ignoreCase = false );

		FieldRuleBuilder<string> Matches( Regex regex );

		FieldRuleBuilder<string> Email( );

		FieldRuleBuilder<string> Url( );

		FieldRuleBuilder<string> OnlyLetters( );

		FieldRuleBuilder<string> OnlyNumbers( );

		FieldRuleBuilder<string> Alphanumeric( );

		FieldRuleBuilder<string> NoSymbols( char[ ]? symbols = null );

		FieldRuleBuilder<string> AvailableCharacters( params char[ ] chars );

		FieldRuleBuilder<string> ForbiddenCharacters( params char[ ] chars );

		FieldRuleBuilder<string> BeOneOf( params string[ ] options );
	}
}