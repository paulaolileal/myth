using Myth.Guard.Rules.Nullables.Booleans;
using Myth.Guard.Rules.Nullables.DateTimes;
using Myth.Guard.Rules.Nullables.Numerics;
using Myth.Rules.Base;
using Myth.Rules.Boooleans;
using Myth.Rules.Collections;
using Myth.Rules.Dates;
using Myth.Rules.DateTimes;
using Myth.Rules.Enums;
using Myth.Rules.Numerics;
using Myth.Rules.Strings;
using System.Net;
using System.Text.RegularExpressions;

namespace Myth.Extensions {

	/// <summary>
	/// Extension methods for <see cref="FluentRuleBuilder{T}"/> providing a complete fluent validation API.
	/// This class contains all the extension methods that enable the fluent validation syntax for different data types
	/// including strings, numbers, collections, dates, booleans, enums, and nullable types.
	/// </summary>
	/// <remarks>
	/// These extension methods allow for chainable validation rules that can be combined using method chaining.
	/// Each method returns a <see cref="FluentRuleBuilder{T}"/> instance, enabling the fluent syntax pattern.
	///
	/// <example>
	/// <code>
	/// builder.For(x => x.Email, r => r
	///     .NotEmpty()
	///     .Email()
	///     .MaximumLength(254)
	///     .WithMessage("Invalid email address"));
	/// </code>
	/// </example>
	/// </remarks>
	public static class FluentRuleBuilderExtensions {

		#region String Extensions

		/// <summary>
		/// Validates that the string is not null, empty, or whitespace-only.
		/// </summary>
		/// <param name="builder">The fluent rule builder instance.</param>
		/// <returns>A <see cref="FluentRuleBuilder{T}"/> for method chaining.</returns>
		/// <example>
		/// <code>
		/// builder.For(x => x.Name, r => r.NotEmpty());
		/// </code>
		/// </example>
		public static FluentRuleBuilder<string> NotEmpty( this FluentRuleBuilder<string> builder ) {
			return builder.AddRule( new NotEmptyStringRule( ) );
		}

		/// <summary>
		/// Validates that the string length is greater than or equal to the specified minimum length.
		/// </summary>
		/// <param name="builder">The fluent rule builder instance.</param>
		/// <param name="length">The minimum required length (inclusive).</param>
		/// <returns>A <see cref="FluentRuleBuilder{T}"/> for method chaining.</returns>
		/// <exception cref="ArgumentException">Thrown when length is negative.</exception>
		/// <example>
		/// <code>
		/// builder.For(x => x.Password, r => r.MinimumLength(8));
		/// </code>
		/// </example>
		public static FluentRuleBuilder<string> MinimumLength( this FluentRuleBuilder<string> builder, int length ) {
			return builder.AddRule( new MinimumLengthRule( length ) );
		}

		/// <summary>
		/// Validates that the string length is less than or equal to the specified maximum length.
		/// </summary>
		/// <param name="builder">The fluent rule builder instance.</param>
		/// <param name="length">The maximum allowed length (inclusive).</param>
		/// <returns>A <see cref="FluentRuleBuilder{T}"/> for method chaining.</returns>
		/// <exception cref="ArgumentException">Thrown when length is negative.</exception>
		/// <example>
		/// <code>
		/// builder.For(x => x.Description, r => r.MaximumLength(500));
		/// </code>
		/// </example>
		public static FluentRuleBuilder<string> MaximumLength( this FluentRuleBuilder<string> builder, int length ) {
			return builder.AddRule( new MaximumLengthRule( length ) );
		}

		/// <summary>
		/// Validates that the string length is within the specified range (inclusive).
		/// </summary>
		/// <param name="builder">The fluent rule builder instance.</param>
		/// <param name="min">The minimum required length (inclusive).</param>
		/// <param name="max">The maximum allowed length (inclusive).</param>
		/// <returns>A <see cref="FluentRuleBuilder{T}"/> for method chaining.</returns>
		/// <exception cref="ArgumentException">Thrown when min is negative, max is negative, or min is greater than max.</exception>
		/// <example>
		/// <code>
		/// builder.For(x => x.Username, r => r.LengthBetween(3, 20));
		/// </code>
		/// </example>
		public static FluentRuleBuilder<string> LengthBetween( this FluentRuleBuilder<string> builder, int min, int max ) {
			return builder.AddRule( new LengthBetweenRule( min, max ) );
		}

		/// <summary>
		/// Validates that the string is a valid email address format.
		/// Uses a comprehensive regex pattern that covers most valid email formats.
		/// </summary>
		/// <param name="builder">The fluent rule builder instance.</param>
		/// <returns>A <see cref="FluentRuleBuilder{T}"/> for method chaining.</returns>
		/// <example>
		/// <code>
		/// builder.For(x => x.Email, r => r.Email());
		/// </code>
		/// </example>
		public static FluentRuleBuilder<string> Email( this FluentRuleBuilder<string> builder ) {
			return builder.AddRule( new EmailRule( ) );
		}

		/// <summary>
		/// Validates that the string is a valid URL format (HTTP, HTTPS, FTP, etc.).
		/// </summary>
		/// <param name="builder">The fluent rule builder instance.</param>
		/// <returns>A <see cref="FluentRuleBuilder{T}"/> for method chaining.</returns>
		/// <example>
		/// <code>
		/// builder.For(x => x.Website, r => r.Url());
		/// </code>
		/// </example>
		public static FluentRuleBuilder<string> Url( this FluentRuleBuilder<string> builder ) {
			return builder.AddRule( new UrlRule( ) );
		}

		/// <summary>
		/// Validates that the string contains only alphabetic characters (letters).
		/// Spaces and other characters are not allowed.
		/// </summary>
		/// <param name="builder">The fluent rule builder instance.</param>
		/// <returns>A <see cref="FluentRuleBuilder{T}"/> for method chaining.</returns>
		/// <example>
		/// <code>
		/// builder.For(x => x.FirstName, r => r.OnlyLetters());
		/// </code>
		/// </example>
		public static FluentRuleBuilder<string> OnlyLetters( this FluentRuleBuilder<string> builder ) {
			return builder.AddRule( new OnlyLettersRule( ) );
		}

		/// <summary>
		/// Validates that the string contains only numeric characters (digits 0-9).
		/// </summary>
		/// <param name="builder">The fluent rule builder instance.</param>
		/// <returns>A <see cref="FluentRuleBuilder{T}"/> for method chaining.</returns>
		/// <example>
		/// <code>
		/// builder.For(x => x.ProductCode, r => r.OnlyNumbers());
		/// </code>
		/// </example>
		public static FluentRuleBuilder<string> OnlyNumbers( this FluentRuleBuilder<string> builder ) {
			return builder.AddRule( new OnlyNumbersRule( ) );
		}

		/// <summary>
		/// Validates that the string contains only alphanumeric characters (letters and digits).
		/// No spaces or special characters are allowed.
		/// </summary>
		/// <param name="builder">The fluent rule builder instance.</param>
		/// <returns>A <see cref="FluentRuleBuilder{T}"/> for method chaining.</returns>
		/// <example>
		/// <code>
		/// builder.For(x => x.Username, r => r.Alphanumeric());
		/// </code>
		/// </example>
		public static FluentRuleBuilder<string> Alphanumeric( this FluentRuleBuilder<string> builder ) {
			return builder.AddRule( new AlphanumericRule( ) );
		}

		/// <summary>
		/// Validates that the string starts with the specified prefix, with optional case-insensitive comparison.
		/// </summary>
		/// <param name="builder">The fluent rule builder instance.</param>
		/// <param name="prefix">The required prefix.</param>
		/// <param name="ignoreCase">If true, performs case-insensitive comparison. Default is false.</param>
		/// <returns>A <see cref="FluentRuleBuilder{T}"/> for method chaining.</returns>
		/// <example>
		/// <code>
		/// builder.For(x => x.PhoneNumber, r => r.StartsWith("+1"));
		/// </code>
		/// </example>
		public static FluentRuleBuilder<string> StartsWith( this FluentRuleBuilder<string> builder, string prefix, bool ignoreCase = false ) {
			return builder.AddRule( new StartsWithRule( prefix, ignoreCase ) );
		}

		/// <summary>
		/// Validates that the string ends with the specified suffix, with optional case-insensitive comparison.
		/// </summary>
		/// <param name="builder">The fluent rule builder instance.</param>
		/// <param name="suffix">The required suffix.</param>
		/// <param name="ignoreCase">If true, performs case-insensitive comparison. Default is false.</param>
		/// <returns>A <see cref="FluentRuleBuilder{T}"/> for method chaining.</returns>
		/// <example>
		/// <code>
		/// builder.For(x => x.EmailAddress, r => r.EndsWith("@company.com"));
		/// </code>
		/// </example>
		public static FluentRuleBuilder<string> EndsWith( this FluentRuleBuilder<string> builder, string suffix, bool ignoreCase = false ) {
			return builder.AddRule( new EndsWithRule( suffix, ignoreCase ) );
		}

		/// <summary>
		/// Validates that the string contains the specified substring, with optional case-insensitive comparison.
		/// </summary>
		/// <param name="builder">The fluent rule builder instance.</param>
		/// <param name="substring">The required substring.</param>
		/// <param name="ignoreCase">If true, performs case-insensitive comparison. Default is false.</param>
		/// <returns>A <see cref="FluentRuleBuilder{T}"/> for method chaining.</returns>
		/// <example>
		/// <code>
		/// builder.For(x => x.Description, r => r.Contains("important"));
		/// </code>
		/// </example>
		public static FluentRuleBuilder<string> Contains( this FluentRuleBuilder<string> builder, string substring, bool ignoreCase = false ) {
			return builder.AddRule( new ContainsRule( substring, ignoreCase ) );
		}

		/// <summary>
		/// Validates that the string matches the specified regular expression pattern.
		/// </summary>
		/// <param name="builder">The fluent rule builder instance.</param>
		/// <param name="regex">The regular expression to match against.</param>
		/// <returns>A <see cref="FluentRuleBuilder{T}"/> for method chaining.</returns>
		/// <exception cref="ArgumentNullException">Thrown when regex is null.</exception>
		/// <example>
		/// <code>
		/// var phoneRegex = new Regex(@"^\+\d{1,3}\d{10,14}$");
		/// builder.For(x => x.Phone, r => r.Matches(phoneRegex));
		/// </code>
		/// </example>
		public static FluentRuleBuilder<string> Matches( this FluentRuleBuilder<string> builder, Regex regex ) {
			return builder.AddRule( new MatchesRule( regex ) );
		}

		/// <summary>
		/// Validates that the string matches one of the specified allowed values.
		/// </summary>
		/// <param name="builder">The fluent rule builder instance.</param>
		/// <param name="options">The allowed string values.</param>
		/// <returns>A <see cref="FluentRuleBuilder{T}"/> for method chaining.</returns>
		/// <example>
		/// <code>
		/// builder.For(x => x.Status, r => r.BeOneOf("Active", "Inactive", "Pending"));
		/// </code>
		/// </example>
		public static FluentRuleBuilder<string> BeOneOf( this FluentRuleBuilder<string> builder, params string[ ] options ) {
			return builder.AddRule( new BeOneOfRule( options ) );
		}

		#endregion String Extensions

		#region Numeric Extensions

		/// <summary>
		/// Validates that the numeric value is greater than the specified minimum value (exclusive).
		/// </summary>
		/// <typeparam name="T">The numeric type being validated.</typeparam>
		/// <param name="builder">The fluent rule builder instance.</param>
		/// <param name="min">The minimum value (exclusive). The actual value must be greater than this.</param>
		/// <returns>A <see cref="FluentRuleBuilder{T}"/> for method chaining.</returns>
		/// <example>
		/// <code>
		/// builder.For(x => x.Age, r => r.GreaterThan(0)); // Age must be positive
		/// </code>
		/// </example>
		public static FluentRuleBuilder<T> GreaterThan<T>( this FluentRuleBuilder<T> builder, T min ) where T : struct, IComparable<T> {
			return builder.AddRule( new GreaterThanRule<T>( min ) );
		}

		/// <summary>
		/// Validates that the numeric value is greater than or equal to the specified minimum value (inclusive).
		/// </summary>
		/// <typeparam name="T">The numeric type being validated.</typeparam>
		/// <param name="builder">The fluent rule builder instance.</param>
		/// <param name="min">The minimum value (inclusive). The actual value must be greater than or equal to this.</param>
		/// <returns>A <see cref="FluentRuleBuilder{T}"/> for method chaining.</returns>
		/// <example>
		/// <code>
		/// builder.For(x => x.Quantity, r => r.GreaterOrEquals(1)); // Quantity must be at least 1
		/// </code>
		/// </example>
		public static FluentRuleBuilder<T> GreaterOrEquals<T>( this FluentRuleBuilder<T> builder, T min ) where T : struct, IComparable<T> {
			return builder.AddRule( new GreaterOrEqualsRule<T>( min ) );
		}

		/// <summary>
		/// Validates that the numeric value is less than the specified maximum value (exclusive).
		/// </summary>
		/// <typeparam name="T">The numeric type being validated.</typeparam>
		/// <param name="builder">The fluent rule builder instance.</param>
		/// <param name="max">The maximum value (exclusive). The actual value must be less than this.</param>
		/// <returns>A <see cref="FluentRuleBuilder{T}"/> for method chaining.</returns>
		/// <example>
		/// <code>
		/// builder.For(x => x.Percentage, r => r.LessThan(100)); // Percentage must be less than 100
		/// </code>
		/// </example>
		public static FluentRuleBuilder<T> LessThan<T>( this FluentRuleBuilder<T> builder, T max ) where T : struct, IComparable<T> {
			return builder.AddRule( new LessThanRule<T>( max ) );
		}

		/// <summary>
		/// Validates that the numeric value is less than or equal to the specified maximum value (inclusive).
		/// </summary>
		/// <typeparam name="T">The numeric type being validated.</typeparam>
		/// <param name="builder">The fluent rule builder instance.</param>
		/// <param name="max">The maximum value (inclusive). The actual value must be less than or equal to this.</param>
		/// <returns>A <see cref="FluentRuleBuilder{T}"/> for method chaining.</returns>
		/// <example>
		/// <code>
		/// builder.For(x => x.Rating, r => r.LessOrEquals(5)); // Rating must be 5 or lower
		/// </code>
		/// </example>
		public static FluentRuleBuilder<T> LessOrEquals<T>( this FluentRuleBuilder<T> builder, T max ) where T : struct, IComparable<T> {
			return builder.AddRule( new LessOrEqualsRule<T>( max ) );
		}

		/// <summary>
		/// Validates that the numeric value is within the specified range (inclusive).
		/// </summary>
		/// <typeparam name="T">The numeric type being validated.</typeparam>
		/// <param name="builder">The fluent rule builder instance.</param>
		/// <param name="min">The minimum value (inclusive).</param>
		/// <param name="max">The maximum value (inclusive).</param>
		/// <returns>A <see cref="FluentRuleBuilder{T}"/> for method chaining.</returns>
		/// <exception cref="ArgumentException">Thrown when min is greater than max.</exception>
		/// <example>
		/// <code>
		/// builder.For(x => x.Age, r => r.Between(18, 65)); // Age must be between 18 and 65
		/// </code>
		/// </example>
		public static FluentRuleBuilder<T> Between<T>( this FluentRuleBuilder<T> builder, T min, T max ) where T : struct, IComparable<T> {
			return builder.AddRule( new BetweenRule<T>( min, max ) );
		}

		/// <summary>
		/// Validates that the numeric value is positive (greater than zero).
		/// </summary>
		/// <typeparam name="T">The numeric type being validated.</typeparam>
		/// <param name="builder">The fluent rule builder instance.</param>
		/// <returns>A <see cref="FluentRuleBuilder{T}"/> for method chaining.</returns>
		/// <example>
		/// <code>
		/// builder.For(x => x.Amount, r => r.Positive()); // Amount must be positive
		/// </code>
		/// </example>
		public static FluentRuleBuilder<T> Positive<T>( this FluentRuleBuilder<T> builder ) where T : struct, IComparable<T> {
			return builder.AddRule( new PositiveRule<T>( ) );
		}

		/// <summary>
		/// Validates that the numeric value is negative (less than zero).
		/// </summary>
		/// <typeparam name="T">The numeric type being validated.</typeparam>
		/// <param name="builder">The fluent rule builder instance.</param>
		/// <returns>A <see cref="FluentRuleBuilder{T}"/> for method chaining.</returns>
		/// <example>
		/// <code>
		/// builder.For(x => x.Debt, r => r.Negative()); // Debt should be negative
		/// </code>
		/// </example>
		public static FluentRuleBuilder<T> Negative<T>( this FluentRuleBuilder<T> builder ) where T : struct, IComparable<T> {
			return builder.AddRule( new NegativeRule<T>( ) );
		}

		/// <summary>
		/// Validates that the numeric value is exactly zero.
		/// </summary>
		/// <typeparam name="T">The numeric type being validated.</typeparam>
		/// <param name="builder">The fluent rule builder instance.</param>
		/// <returns>A <see cref="FluentRuleBuilder{T}"/> for method chaining.</returns>
		/// <example>
		/// <code>
		/// builder.For(x => x.Balance, r => r.Zero()); // Balance must be exactly zero
		/// </code>
		/// </example>
		public static FluentRuleBuilder<T> Zero<T>( this FluentRuleBuilder<T> builder ) where T : struct, IComparable<T> {
			return builder.AddRule( new ZeroRule<T>( ) );
		}

		/// <summary>
		/// Validates that the numeric value is not zero (either positive or negative).
		/// </summary>
		/// <typeparam name="T">The numeric type being validated.</typeparam>
		/// <param name="builder">The fluent rule builder instance.</param>
		/// <returns>A <see cref="FluentRuleBuilder{T}"/> for method chaining.</returns>
		/// <example>
		/// <code>
		/// builder.For(x => x.Divisor, r => r.NotZero()); // Divisor cannot be zero
		/// </code>
		/// </example>
		public static FluentRuleBuilder<T> NotZero<T>( this FluentRuleBuilder<T> builder ) where T : struct, IComparable<T> {
			return builder.AddRule( new NotZeroRule<T>( ) );
		}

		#endregion Numeric Extensions

		#region Boolean Extensions

		/// <summary>
		/// Validates that the boolean value is true.
		/// </summary>
		/// <param name="builder">The fluent rule builder instance.</param>
		/// <returns>A <see cref="FluentRuleBuilder{T}"/> for method chaining.</returns>
		/// <example>
		/// <code>
		/// builder.For(x => x.IsAccepted, r => r.IsTrue()); // Must be accepted
		/// </code>
		/// </example>
		public static FluentRuleBuilder<bool> IsTrue( this FluentRuleBuilder<bool> builder ) {
			return builder.AddRule( new IsTrueRule( ) );
		}

		/// <summary>
		/// Validates that the boolean value is false.
		/// </summary>
		/// <param name="builder">The fluent rule builder instance.</param>
		/// <returns>A <see cref="FluentRuleBuilder{T}"/> for method chaining.</returns>
		/// <example>
		/// <code>
		/// builder.For(x => x.IsDeleted, r => r.IsFalse()); // Must not be deleted
		/// </code>
		/// </example>
		public static FluentRuleBuilder<bool> IsFalse( this FluentRuleBuilder<bool> builder ) {
			return builder.AddRule( new IsFalseRule( ) );
		}

		#endregion Boolean Extensions

		#region DateTime Extensions

		public static FluentRuleBuilder<DateTime> Past( this FluentRuleBuilder<DateTime> builder ) {
			return builder.AddRule( new PastRule( ) );
		}

		public static FluentRuleBuilder<DateTime> Future( this FluentRuleBuilder<DateTime> builder ) {
			return builder.AddRule( new FutureRule( ) );
		}

		public static FluentRuleBuilder<DateTime> After( this FluentRuleBuilder<DateTime> builder, DateTime minDate ) {
			return builder.AddRule( new AfterDateRule( minDate ) );
		}

		public static FluentRuleBuilder<DateTime> Before( this FluentRuleBuilder<DateTime> builder, DateTime maxDate ) {
			return builder.AddRule( new BeforeDateRule( maxDate ) );
		}

		public static FluentRuleBuilder<DateTime> Between( this FluentRuleBuilder<DateTime> builder, DateTime minDate, DateTime maxDate ) {
			return builder.AddRule( new BetweenDateRule( minDate, maxDate ) );
		}

		public static FluentRuleBuilder<DateTime> Today( this FluentRuleBuilder<DateTime> builder ) {
			return builder.AddRule( new TodayRule( ) );
		}

		#endregion DateTime Extensions

		#region DateOnly Extensions

		public static FluentRuleBuilder<DateOnly> BeforeOrEquals( this FluentRuleBuilder<DateOnly> builder, DateOnly maxDate ) {
			return builder.AddRule( new BeforeOrEqualsDateOnlyRule( maxDate ) );
		}

		public static FluentRuleBuilder<DateOnly> Between( this FluentRuleBuilder<DateOnly> builder, DateOnly minDate, DateOnly maxDate ) {
			return builder.AddRule( new BetweenDateOnlyRule( minDate, maxDate ) );
		}

		public static FluentRuleBuilder<DateOnly> After( this FluentRuleBuilder<DateOnly> builder, DateOnly minDate ) {
			return builder.AddRule( new AfterDateOnlyRule( minDate ) );
		}

		public static FluentRuleBuilder<DateOnly> Before( this FluentRuleBuilder<DateOnly> builder, DateOnly maxDate ) {
			return builder.AddRule( new BeforeDateOnlyRule( maxDate ) );
		}

		public static FluentRuleBuilder<DateOnly> Past( this FluentRuleBuilder<DateOnly> builder ) {
			return builder.AddRule( new PastDateOnlyRule( ) );
		}

		public static FluentRuleBuilder<DateOnly> Future( this FluentRuleBuilder<DateOnly> builder ) {
			return builder.AddRule( new FutureDateOnlyRule( ) );
		}

		public static FluentRuleBuilder<DateOnly> Today( this FluentRuleBuilder<DateOnly> builder ) {
			return builder.AddRule( new TodayDateOnlyRule( ) );
		}

		#endregion DateOnly Extensions

		#region Collection Extensions

		public static FluentRuleBuilder<IEnumerable<T>> NotEmpty<T>( this FluentRuleBuilder<IEnumerable<T>> builder ) {
			return builder.AddRule( new NotEmptyCollectionRule<T>( ) );
		}

		public static FluentRuleBuilder<IEnumerable<T>> CountBetween<T>( this FluentRuleBuilder<IEnumerable<T>> builder, int min, int max ) {
			return builder.AddRule( new CountBetweenRule<T>( min, max ) );
		}

		public static FluentRuleBuilder<IEnumerable<T>> CountGreaterThan<T>( this FluentRuleBuilder<IEnumerable<T>> builder, int min ) {
			return builder.AddRule( new CountGreaterThanRule<T>( min ) );
		}

		public static FluentRuleBuilder<IEnumerable<T>> CountLessThan<T>( this FluentRuleBuilder<IEnumerable<T>> builder, int max ) {
			return builder.AddRule( new CountLessThanRule<T>( max ) );
		}

		public static FluentRuleBuilder<IEnumerable<T>> All<T>( this FluentRuleBuilder<IEnumerable<T>> builder, Func<T, bool> predicate ) {
			return builder.AddRule( new AllRule<T>( predicate ) );
		}

		public static FluentRuleBuilder<IEnumerable<T>> Any<T>( this FluentRuleBuilder<IEnumerable<T>> builder, Func<T, bool> predicate ) {
			return builder.AddRule( new AnyRule<T>( predicate ) );
		}

		public static FluentRuleBuilder<IEnumerable<T>> None<T>( this FluentRuleBuilder<IEnumerable<T>> builder, Func<T, bool> predicate ) {
			return builder.AddRule( new NoneRule<T>( predicate ) );
		}

		public static FluentRuleBuilder<IEnumerable<T>> Distinct<T>( this FluentRuleBuilder<IEnumerable<T>> builder ) {
			return builder.AddRule( new DistinctRule<T>( ) );
		}

		public static FluentRuleBuilder<IEnumerable<T>> DistinctBy<T, TKey>( this FluentRuleBuilder<IEnumerable<T>> builder, Func<T, TKey> keySelector ) {
			return builder.AddRule( new DistinctByRule<T, TKey>( keySelector ) );
		}

		#endregion Collection Extensions

		#region Enum Extensions

		public static FluentRuleBuilder<TEnum> BeInEnum<TEnum>( this FluentRuleBuilder<TEnum> builder ) where TEnum : struct, Enum {
			return builder.AddRule( new BeInEnumRule<TEnum>( ) );
		}

		#endregion Enum Extensions

		#region Generic Extensions (already implemented in FluentRuleBuilder base class, but adding for completeness)

		public static FluentRuleBuilder<T> NotNull<T>( this FluentRuleBuilder<T> builder ) {
			return builder.NotNull( );
		}

		public static FluentRuleBuilder<T> BeNull<T>( this FluentRuleBuilder<T> builder ) {
			return builder.BeNull( );
		}

		public static FluentRuleBuilder<T> EqualsTo<T>( this FluentRuleBuilder<T> builder, T value ) {
			return builder.EqualsTo( value );
		}

		public static FluentRuleBuilder<T> NotEqualsTo<T>( this FluentRuleBuilder<T> builder, T value ) {
			return builder.NotEqualsTo( value );
		}

		public static FluentRuleBuilder<T> Respect<T>( this FluentRuleBuilder<T> builder, Func<T, bool> predicate ) {
			return builder.Respect( predicate );
		}

		public static FluentRuleBuilder<T> RespectAsync<T>( this FluentRuleBuilder<T> builder, Func<T, CancellationToken, IServiceProvider, Task<bool>> predicate ) {
			return builder.RespectAsync( predicate );
		}

		public static FluentRuleBuilder<T> BeDefault<T>( this FluentRuleBuilder<T> builder ) {
			return builder.BeDefault( );
		}

		public static FluentRuleBuilder<T> NotDefault<T>( this FluentRuleBuilder<T> builder ) {
			return builder.NotDefault( );
		}

		#endregion Generic Extensions (already implemented in FluentRuleBuilder base class, but adding for completeness)

		#region Nullable Extensions

		// DateTime? Extensions
		public static FluentRuleBuilder<DateTime?> Future( this FluentRuleBuilder<DateTime?> builder ) {
			return builder.AddRule( new NullableFutureRule( ) );
		}

		public static FluentRuleBuilder<DateTime?> Past( this FluentRuleBuilder<DateTime?> builder ) {
			return builder.AddRule( new NullablePastRule( ) );
		}

		public static FluentRuleBuilder<DateTime?> Today( this FluentRuleBuilder<DateTime?> builder ) {
			return builder.AddRule( new NullableTodayRule( ) );
		}

		public static FluentRuleBuilder<DateTime?> After( this FluentRuleBuilder<DateTime?> builder, DateTime dateTime ) {
			return builder.AddRule( new NullableAfterRule( dateTime ) );
		}

		public static FluentRuleBuilder<DateTime?> Before( this FluentRuleBuilder<DateTime?> builder, DateTime dateTime ) {
			return builder.AddRule( new NullableBeforeRule( dateTime ) );
		}

		public static FluentRuleBuilder<DateTime?> Between( this FluentRuleBuilder<DateTime?> builder, DateTime start, DateTime end ) {
			return builder.AddRule( new NullableBetweenRule( start, end ) );
		}

		public static FluentRuleBuilder<DateTime?> AfterOrEquals( this FluentRuleBuilder<DateTime?> builder, DateTime dateTime ) {
			return builder.AddRule( new NullableAfterOrEqualsRule( dateTime ) );
		}

		public static FluentRuleBuilder<DateTime?> BeforeOrEquals( this FluentRuleBuilder<DateTime?> builder, DateTime dateTime ) {
			return builder.AddRule( new NullableBeforeOrEqualsRule( dateTime ) );
		}

		// Numeric? Extensions
		public static FluentRuleBuilder<T?> GreaterThan<T>( this FluentRuleBuilder<T?> builder, T value ) where T : struct, IComparable<T> {
			return builder.AddRule( new NullableGreaterThanRule<T>( value ) );
		}

		public static FluentRuleBuilder<T?> GreaterOrEquals<T>( this FluentRuleBuilder<T?> builder, T value ) where T : struct, IComparable<T> {
			return builder.AddRule( new NullableGreaterOrEqualsRule<T>( value ) );
		}

		public static FluentRuleBuilder<T?> LessThan<T>( this FluentRuleBuilder<T?> builder, T value ) where T : struct, IComparable<T> {
			return builder.AddRule( new NullableLessThanRule<T>( value ) );
		}

		public static FluentRuleBuilder<T?> LessOrEquals<T>( this FluentRuleBuilder<T?> builder, T value ) where T : struct, IComparable<T> {
			return builder.AddRule( new NullableLessOrEqualsRule<T>( value ) );
		}

		public static FluentRuleBuilder<T?> Between<T>( this FluentRuleBuilder<T?> builder, T min, T max ) where T : struct, IComparable<T> {
			return builder.AddRule( new NullableBetweenRule<T>( min, max ) );
		}

		public static FluentRuleBuilder<T?> Positive<T>( this FluentRuleBuilder<T?> builder ) where T : struct, IComparable<T> {
			return builder.AddRule( new NullablePositiveRule<T>( ) );
		}

		public static FluentRuleBuilder<T?> Negative<T>( this FluentRuleBuilder<T?> builder ) where T : struct, IComparable<T> {
			return builder.AddRule( new NullableNegativeRule<T>( ) );
		}

		public static FluentRuleBuilder<T?> Zero<T>( this FluentRuleBuilder<T?> builder ) where T : struct, IComparable<T> {
			return builder.AddRule( new NullableZeroRule<T>( ) );
		}

		public static FluentRuleBuilder<T?> NotZero<T>( this FluentRuleBuilder<T?> builder ) where T : struct, IComparable<T> {
			return builder.AddRule( new NullableNotZeroRule<T>( ) );
		}

		// Boolean? Extensions
		public static FluentRuleBuilder<bool?> IsTrue( this FluentRuleBuilder<bool?> builder ) {
			return builder.AddRule( new NullableIsTrueRule( ) );
		}

		public static FluentRuleBuilder<bool?> IsFalse( this FluentRuleBuilder<bool?> builder ) {
			return builder.AddRule( new NullableIsFalseRule( ) );
		}

		#endregion Nullable Extensions

		#region Conditional Extensions (When/Unless) - Now functional!

		public static FluentRuleBuilder<T> When<T>( this FluentRuleBuilder<T> builder, Func<T, bool> condition ) {
			return builder.When( condition );
		}

		public static FluentRuleBuilder<T> Unless<T>( this FluentRuleBuilder<T> builder, Func<T, bool> condition ) {
			return builder.Unless( condition );
		}

		public static FluentRuleBuilder<T> When<T, TEntity>( this FluentRuleBuilder<T> builder, Func<TEntity, bool> entityCondition ) {
			return builder.WhenEntity( entity => entityCondition( ( TEntity )entity ) );
		}

		public static FluentRuleBuilder<T> Unless<T, TEntity>( this FluentRuleBuilder<T> builder, Func<TEntity, bool> entityCondition ) {
			return builder.UnlessEntity( entity => entityCondition( ( TEntity )entity ) );
		}

		#endregion Conditional Extensions (When/Unless) - Now functional!

		#region Chaining Support Methods - Now functional!

		public static FluentRuleBuilder<T> WithMessage<T>( this FluentRuleBuilder<T> builder, string message ) {
			return builder.WithMessage( message );
		}

		public static FluentRuleBuilder<T> WithMessage<T>( this FluentRuleBuilder<T> builder, Func<T, string> messageFunc ) {
			return builder.WithMessage( messageFunc );
		}

		public static FluentRuleBuilder<T> WithCode<T>( this FluentRuleBuilder<T> builder, string code ) {
			return builder.WithCode( code );
		}

		public static FluentRuleBuilder<T> WithStatusCode<T>( this FluentRuleBuilder<T> builder, HttpStatusCode statusCode ) {
			return builder.WithStatusCode( statusCode );
		}

		public static FluentRuleBuilder<T> SetStopOnFailure<T>( this FluentRuleBuilder<T> builder ) {
			return builder.SetStopOnFailure( );
		}

		#endregion Chaining Support Methods - Now functional!
	}
}