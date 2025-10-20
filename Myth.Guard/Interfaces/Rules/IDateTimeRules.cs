using Myth.Rules.Base;

namespace Myth.Interfaces.Rules {

	/// <summary>
	/// Provides validation rules specifically designed for DateTime values,
	/// including temporal comparisons and date range validation.
	/// </summary>
	public interface IDateTimeRules : IRuleConfigurator<DateTime> {

		/// <summary>
		/// Validates that the DateTime value is after the specified date (exclusive).
		/// </summary>
		/// <param name="date">The comparison date. The value must be after this date.</param>
		/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
		/// <example>
		/// <code>
		/// builder.For(x => x.StartDate, r => r.After(DateTime.Today)); // Must be after today
		/// builder.For(x => x.EventDate, r => r.After(new DateTime(2023, 1, 1))); // Must be after Jan 1, 2023
		/// </code>
		/// </example>
		FieldRuleBuilder<DateTime> After( DateTime date );

		/// <summary>
		/// Validates that the DateTime value is after or equal to the specified date (inclusive).
		/// </summary>
		/// <param name="date">The comparison date. The value must be on or after this date.</param>
		/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
		/// <example>
		/// <code>
		/// builder.For(x => x.ScheduledDate, r => r.AfterOrEquals(DateTime.Today)); // Can be today or later
		/// builder.For(x => x.BirthDate, r => r.AfterOrEquals(new DateTime(1900, 1, 1))); // Must be 1900 or later
		/// </code>
		/// </example>
		FieldRuleBuilder<DateTime> AfterOrEquals( DateTime date );

		/// <summary>
		/// Validates that the DateTime value is before the specified date (exclusive).
		/// </summary>
		/// <param name="date">The comparison date. The value must be before this date.</param>
		/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
		/// <example>
		/// <code>
		/// builder.For(x => x.ExpiryDate, r => r.Before(DateTime.Today.AddYears(1))); // Must expire within a year
		/// builder.For(x => x.EventDate, r => r.Before(DateTime.Now.AddDays(30))); // Must be within 30 days
		/// </code>
		/// </example>
		FieldRuleBuilder<DateTime> Before( DateTime date );

		/// <summary>
		/// Validates that the DateTime value is before or equal to the specified date (inclusive).
		/// </summary>
		/// <param name="date">The comparison date. The value must be on or before this date.</param>
		/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
		/// <example>
		/// <code>
		/// builder.For(x => x.DeadlineDate, r => r.BeforeOrEquals(DateTime.Today)); // Deadline can be today or earlier
		/// builder.For(x => x.CompletionDate, r => r.BeforeOrEquals(DateTime.Now)); // Can be completed up to now
		/// </code>
		/// </example>
		FieldRuleBuilder<DateTime> BeforeOrEquals( DateTime date );

		/// <summary>
		/// Validates that the DateTime value is within the specified date range (inclusive).
		/// </summary>
		/// <param name="min">The minimum date (inclusive).</param>
		/// <param name="max">The maximum date (inclusive).</param>
		/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
		/// <exception cref="ArgumentException">Thrown when min is greater than max.</exception>
		/// <example>
		/// <code>
		/// var validRange = (DateTime.Today, DateTime.Today.AddDays(30));
		/// builder.For(x => x.EventDate, r => r.Between(validRange.Item1, validRange.Item2)); // Must be within 30 days
		/// builder.For(x => x.BirthDate, r => r.Between(new DateTime(1920, 1, 1), DateTime.Today)); // Valid birth date range
		/// </code>
		/// </example>
		FieldRuleBuilder<DateTime> Between( DateTime min, DateTime max );

		/// <summary>
		/// Validates that the DateTime value represents today's date (ignoring time component).
		/// </summary>
		/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
		/// <example>
		/// <code>
		/// builder.For(x => x.TransactionDate, r => r.Today()); // Transaction must be today
		/// builder.For(x => x.LogDate, r => r.Today()); // Log entry must be from today
		/// </code>
		/// </example>
		FieldRuleBuilder<DateTime> Today( );

		/// <summary>
		/// Validates that the DateTime value is in the past (before the current date and time).
		/// </summary>
		/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
		/// <example>
		/// <code>
		/// builder.For(x => x.BirthDate, r => r.Past()); // Birth date must be in the past
		/// builder.For(x => x.CompletedDate, r => r.Past()); // Completion date must be in the past
		/// </code>
		/// </example>
		FieldRuleBuilder<DateTime> Past( );

		/// <summary>
		/// Validates that the DateTime value is in the future (after the current date and time).
		/// </summary>
		/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
		/// <example>
		/// <code>
		/// builder.For(x => x.AppointmentDate, r => r.Future()); // Appointment must be in the future
		/// builder.For(x => x.ExpiryDate, r => r.Future()); // Expiry date must be in the future
		/// </code>
		/// </example>
		FieldRuleBuilder<DateTime> Future( );
	}
}