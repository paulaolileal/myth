using System.Text.RegularExpressions;
using Myth.Rules.Base;

namespace Myth.Interfaces.Rules;

/// <summary>
/// Provides validation rules specifically designed for string values, including length validation,
/// format validation, and character composition validation.
/// </summary>
public interface IStringRules : IRuleConfigurator<string> {

	/// <summary>
	/// Validates that the string is not null, empty, or whitespace-only.
	/// </summary>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.Name, r => r.NotEmpty());
	/// </code>
	/// </example>
	FieldRuleBuilder<string> NotEmpty( );

	/// <summary>
	/// Validates that the string length is greater than or equal to the specified minimum length.
	/// </summary>
	/// <param name="length">The minimum required length (inclusive).</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <exception cref="ArgumentException">Thrown when length is negative.</exception>
	/// <example>
	/// <code>
	/// builder.For(x => x.Password, r => r.MinimumLength(8));
	/// </code>
	/// </example>
	FieldRuleBuilder<string> MinimumLength( int length );

	/// <summary>
	/// Validates that the string length is less than or equal to the specified maximum length.
	/// </summary>
	/// <param name="length">The maximum allowed length (inclusive).</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <exception cref="ArgumentException">Thrown when length is negative.</exception>
	/// <example>
	/// <code>
	/// builder.For(x => x.Description, r => r.MaximumLength(500));
	/// </code>
	/// </example>
	FieldRuleBuilder<string> MaximumLength( int length );

	/// <summary>
	/// Validates that the string length is within the specified range (inclusive).
	/// </summary>
	/// <param name="min">The minimum required length (inclusive).</param>
	/// <param name="max">The maximum allowed length (inclusive).</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <exception cref="ArgumentException">Thrown when min is negative, max is negative, or min is greater than max.</exception>
	/// <example>
	/// <code>
	/// builder.For(x => x.Username, r => r.LengthBetween(3, 20));
	/// </code>
	/// </example>
	FieldRuleBuilder<string> LengthBetween( int min, int max );

	/// <summary>
	/// Validates that the string equals the expected value, with optional case-insensitive comparison.
	/// </summary>
	/// <param name="expected">The expected string value.</param>
	/// <param name="ignoreCase">If true, performs case-insensitive comparison. Default is false.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.Status, r => r.EqualsTo("ACTIVE", ignoreCase: true));
	/// </code>
	/// </example>
	FieldRuleBuilder<string> EqualsTo( string expected, bool ignoreCase = false );

	/// <summary>
	/// Validates that the string starts with the specified prefix, with optional case-insensitive comparison.
	/// </summary>
	/// <param name="prefix">The required prefix.</param>
	/// <param name="ignoreCase">If true, performs case-insensitive comparison. Default is false.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.PhoneNumber, r => r.StartsWith("+1"));
	/// </code>
	/// </example>
	FieldRuleBuilder<string> StartsWith( string prefix, bool ignoreCase = false );

	/// <summary>
	/// Validates that the string ends with the specified suffix, with optional case-insensitive comparison.
	/// </summary>
	/// <param name="suffix">The required suffix.</param>
	/// <param name="ignoreCase">If true, performs case-insensitive comparison. Default is false.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.EmailAddress, r => r.EndsWith("@company.com"));
	/// </code>
	/// </example>
	FieldRuleBuilder<string> EndsWith( string suffix, bool ignoreCase = false );

	/// <summary>
	/// Validates that the string contains the specified substring, with optional case-insensitive comparison.
	/// </summary>
	/// <param name="substring">The required substring.</param>
	/// <param name="ignoreCase">If true, performs case-insensitive comparison. Default is false.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.Description, r => r.Contains("important"));
	/// </code>
	/// </example>
	FieldRuleBuilder<string> Contains( string substring, bool ignoreCase = false );

	/// <summary>
	/// Validates that the string matches the specified regular expression pattern.
	/// </summary>
	/// <param name="regex">The regular expression to match against.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <exception cref="ArgumentNullException">Thrown when regex is null.</exception>
	/// <example>
	/// <code>
	/// var phoneRegex = new Regex(@"^\+\d{1,3}\d{10,14}$");
	/// builder.For(x => x.Phone, r => r.Matches(phoneRegex));
	/// </code>
	/// </example>
	FieldRuleBuilder<string> Matches( Regex regex );

	/// <summary>
	/// Validates that the string is a valid email address format.
	/// Uses a comprehensive regex pattern that covers most valid email formats.
	/// </summary>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.Email, r => r.Email());
	/// </code>
	/// </example>
	FieldRuleBuilder<string> Email( );

	/// <summary>
	/// Validates that the string is a valid URL format (HTTP, HTTPS, FTP, etc.).
	/// </summary>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.Website, r => r.Url());
	/// </code>
	/// </example>
	FieldRuleBuilder<string> Url( );

	/// <summary>
	/// Validates that the string contains only alphabetic characters (letters).
	/// Spaces and other characters are not allowed.
	/// </summary>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.FirstName, r => r.OnlyLetters());
	/// </code>
	/// </example>
	FieldRuleBuilder<string> OnlyLetters( );

	/// <summary>
	/// Validates that the string contains only numeric characters (digits 0-9).
	/// </summary>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.ProductCode, r => r.OnlyNumbers());
	/// </code>
	/// </example>
	FieldRuleBuilder<string> OnlyNumbers( );

	/// <summary>
	/// Validates that the string contains only alphanumeric characters (letters and digits).
	/// No spaces or special characters are allowed.
	/// </summary>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.Username, r => r.Alphanumeric());
	/// </code>
	/// </example>
	FieldRuleBuilder<string> Alphanumeric( );

	/// <summary>
	/// Validates that the string does not contain any symbol characters.
	/// Optionally specify which specific symbols to check for.
	/// </summary>
	/// <param name="symbols">Specific symbols to forbid. If null, checks against all common symbols.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.CleanText, r => r.NoSymbols());
	/// builder.For(x => x.SafeInput, r => r.NoSymbols(['!', '@', '#']));
	/// </code>
	/// </example>
	FieldRuleBuilder<string> NoSymbols( char[ ]? symbols = null );

	/// <summary>
	/// Validates that the string contains only characters from the specified allowed character set.
	/// </summary>
	/// <param name="chars">The characters that are allowed in the string.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.HexCode, r => r.AvailableCharacters('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'));
	/// </code>
	/// </example>
	FieldRuleBuilder<string> AvailableCharacters( params char[ ] chars );

	/// <summary>
	/// Validates that the string does not contain any of the specified forbidden characters.
	/// </summary>
	/// <param name="chars">The characters that are forbidden in the string.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.SafeText, r => r.ForbiddenCharacters('<', '>', '"', '\''));
	/// </code>
	/// </example>
	FieldRuleBuilder<string> ForbiddenCharacters( params char[ ] chars );

	/// <summary>
	/// Validates that the string matches one of the specified allowed values.
	/// </summary>
	/// <param name="options">The allowed string values.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.Status, r => r.BeOneOf("Active", "Inactive", "Pending"));
	/// </code>
	/// </example>
	FieldRuleBuilder<string> BeOneOf( params string[ ] options );
}
