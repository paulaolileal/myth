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
	/// Extension methods for FluentRuleBuilder providing complete fluent validation API
	/// </summary>
	public static class FluentRuleBuilderExtensions {

		#region String Extensions

		public static FluentRuleBuilder<string> NotEmpty( this FluentRuleBuilder<string> builder ) {
			return builder.AddRule( new NotEmptyStringRule( ) );
		}

		public static FluentRuleBuilder<string> MinimumLength( this FluentRuleBuilder<string> builder, int length ) {
			return builder.AddRule( new MinimumLengthRule( length ) );
		}

		public static FluentRuleBuilder<string> MaximumLength( this FluentRuleBuilder<string> builder, int length ) {
			return builder.AddRule( new MaximumLengthRule( length ) );
		}

		public static FluentRuleBuilder<string> LengthBetween( this FluentRuleBuilder<string> builder, int min, int max ) {
			return builder.AddRule( new LengthBetweenRule( min, max ) );
		}

		public static FluentRuleBuilder<string> Email( this FluentRuleBuilder<string> builder ) {
			return builder.AddRule( new EmailRule( ) );
		}

		public static FluentRuleBuilder<string> Url( this FluentRuleBuilder<string> builder ) {
			return builder.AddRule( new UrlRule( ) );
		}

		public static FluentRuleBuilder<string> OnlyLetters( this FluentRuleBuilder<string> builder ) {
			return builder.AddRule( new OnlyLettersRule( ) );
		}

		public static FluentRuleBuilder<string> OnlyNumbers( this FluentRuleBuilder<string> builder ) {
			return builder.AddRule( new OnlyNumbersRule( ) );
		}

		public static FluentRuleBuilder<string> Alphanumeric( this FluentRuleBuilder<string> builder ) {
			return builder.AddRule( new AlphanumericRule( ) );
		}

		public static FluentRuleBuilder<string> StartsWith( this FluentRuleBuilder<string> builder, string prefix, bool ignoreCase = false ) {
			return builder.AddRule( new StartsWithRule( prefix, ignoreCase ) );
		}

		public static FluentRuleBuilder<string> EndsWith( this FluentRuleBuilder<string> builder, string suffix, bool ignoreCase = false ) {
			return builder.AddRule( new EndsWithRule( suffix, ignoreCase ) );
		}

		public static FluentRuleBuilder<string> Contains( this FluentRuleBuilder<string> builder, string substring, bool ignoreCase = false ) {
			return builder.AddRule( new ContainsRule( substring, ignoreCase ) );
		}

		public static FluentRuleBuilder<string> Matches( this FluentRuleBuilder<string> builder, Regex regex ) {
			return builder.AddRule( new MatchesRule( regex ) );
		}

		public static FluentRuleBuilder<string> BeOneOf( this FluentRuleBuilder<string> builder, params string[ ] options ) {
			return builder.AddRule( new BeOneOfRule( options ) );
		}

		#endregion String Extensions

		#region Numeric Extensions

		public static FluentRuleBuilder<T> GreaterThan<T>( this FluentRuleBuilder<T> builder, T min ) where T : struct, IComparable<T> {
			return builder.AddRule( new GreaterThanRule<T>( min ) );
		}

		public static FluentRuleBuilder<T> GreaterOrEquals<T>( this FluentRuleBuilder<T> builder, T min ) where T : struct, IComparable<T> {
			return builder.AddRule( new GreaterOrEqualsRule<T>( min ) );
		}

		public static FluentRuleBuilder<T> LessThan<T>( this FluentRuleBuilder<T> builder, T max ) where T : struct, IComparable<T> {
			return builder.AddRule( new LessThanRule<T>( max ) );
		}

		public static FluentRuleBuilder<T> LessOrEquals<T>( this FluentRuleBuilder<T> builder, T max ) where T : struct, IComparable<T> {
			return builder.AddRule( new LessOrEqualsRule<T>( max ) );
		}

		public static FluentRuleBuilder<T> Between<T>( this FluentRuleBuilder<T> builder, T min, T max ) where T : struct, IComparable<T> {
			return builder.AddRule( new BetweenRule<T>( min, max ) );
		}

		public static FluentRuleBuilder<T> Positive<T>( this FluentRuleBuilder<T> builder ) where T : struct, IComparable<T> {
			return builder.AddRule( new PositiveRule<T>( ) );
		}

		public static FluentRuleBuilder<T> Negative<T>( this FluentRuleBuilder<T> builder ) where T : struct, IComparable<T> {
			return builder.AddRule( new NegativeRule<T>( ) );
		}

		public static FluentRuleBuilder<T> Zero<T>( this FluentRuleBuilder<T> builder ) where T : struct, IComparable<T> {
			return builder.AddRule( new ZeroRule<T>( ) );
		}

		public static FluentRuleBuilder<T> NotZero<T>( this FluentRuleBuilder<T> builder ) where T : struct, IComparable<T> {
			return builder.AddRule( new NotZeroRule<T>( ) );
		}

		#endregion Numeric Extensions

		#region Boolean Extensions

		public static FluentRuleBuilder<bool> IsTrue( this FluentRuleBuilder<bool> builder ) {
			return builder.AddRule( new IsTrueRule( ) );
		}

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