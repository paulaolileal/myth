using Myth.Rules.Base;

namespace Myth.Interfaces.Rules;

/// <summary>
/// Provides validation rules specifically designed for numeric values, including range validation,
/// comparison validation, and sign validation.
/// </summary>
/// <typeparam name="T">The numeric type being validated (e.g., int, decimal, double, etc.).</typeparam>
public interface INumericRules<T> : IRuleConfigurator<T> where T : struct, IComparable<T> {

	/// <summary>
	/// Validates that the numeric value is greater than the specified minimum value (exclusive).
	/// </summary>
	/// <param name="min">The minimum value (exclusive). The actual value must be greater than this.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.Age, r => r.GreaterThan(0)); // Age must be positive
	/// builder.For(x => x.Score, r => r.GreaterThan(50.0)); // Score must be greater than 50
	/// </code>
	/// </example>
	FieldRuleBuilder<T> GreaterThan( T min );

	/// <summary>
	/// Validates that the numeric value is greater than or equal to the specified minimum value (inclusive).
	/// </summary>
	/// <param name="min">The minimum value (inclusive). The actual value must be greater than or equal to this.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.Quantity, r => r.GreaterOrEquals(1)); // Quantity must be at least 1
	/// builder.For(x => x.Temperature, r => r.GreaterOrEquals(-40.0)); // Temperature must be at least -40
	/// </code>
	/// </example>
	FieldRuleBuilder<T> GreaterOrEquals( T min );

	/// <summary>
	/// Validates that the numeric value is less than the specified maximum value (exclusive).
	/// </summary>
	/// <param name="max">The maximum value (exclusive). The actual value must be less than this.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.Percentage, r => r.LessThan(100)); // Percentage must be less than 100
	/// builder.For(x => x.Price, r => r.LessThan(1000.00m)); // Price must be under $1000
	/// </code>
	/// </example>
	FieldRuleBuilder<T> LessThan( T max );

	/// <summary>
	/// Validates that the numeric value is less than or equal to the specified maximum value (inclusive).
	/// </summary>
	/// <param name="max">The maximum value (inclusive). The actual value must be less than or equal to this.</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.Rating, r => r.LessOrEquals(5)); // Rating must be 5 or lower
	/// builder.For(x => x.Discount, r => r.LessOrEquals(0.5)); // Discount must be 50% or less
	/// </code>
	/// </example>
	FieldRuleBuilder<T> LessOrEquals( T max );

	/// <summary>
	/// Validates that the numeric value is within the specified range (inclusive).
	/// </summary>
	/// <param name="min">The minimum value (inclusive).</param>
	/// <param name="max">The maximum value (inclusive).</param>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <exception cref="ArgumentException">Thrown when min is greater than max.</exception>
	/// <example>
	/// <code>
	/// builder.For(x => x.Age, r => r.Between(18, 65)); // Age must be between 18 and 65
	/// builder.For(x => x.Score, r => r.Between(0.0, 100.0)); // Score must be between 0 and 100
	/// </code>
	/// </example>
	FieldRuleBuilder<T> Between( T min, T max );

	/// <summary>
	/// Validates that the numeric value is positive (greater than zero).
	/// </summary>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.Amount, r => r.Positive()); // Amount must be positive
	/// builder.For(x => x.Length, r => r.Positive()); // Length must be positive
	/// </code>
	/// </example>
	FieldRuleBuilder<T> Positive( );

	/// <summary>
	/// Validates that the numeric value is negative (less than zero).
	/// </summary>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.Debt, r => r.Negative()); // Debt should be negative
	/// builder.For(x => x.TemperatureChange, r => r.Negative()); // Change should be negative
	/// </code>
	/// </example>
	FieldRuleBuilder<T> Negative( );

	/// <summary>
	/// Validates that the numeric value is exactly zero.
	/// </summary>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.Balance, r => r.Zero()); // Balance must be exactly zero
	/// builder.For(x => x.Remainder, r => r.Zero()); // Remainder must be zero
	/// </code>
	/// </example>
	FieldRuleBuilder<T> Zero( );

	/// <summary>
	/// Validates that the numeric value is not zero (either positive or negative).
	/// </summary>
	/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.For(x => x.Divisor, r => r.NotZero()); // Divisor cannot be zero
	/// builder.For(x => x.Speed, r => r.NotZero()); // Speed must be non-zero
	/// </code>
	/// </example>
	FieldRuleBuilder<T> NotZero( );
}
