using Myth.Models;

namespace Myth.Validation;

/// <summary>
/// Base interface for standalone validation builders providing core validation functionality
/// </summary>
/// <typeparam name="T">The type of value being validated</typeparam>
public interface IStandaloneValidationBuilder<T> : IStandaloneValidationBuilder {

	/// <summary>
	/// Adds a validation rule that the value must not be null
	/// </summary>
	/// <returns>The validation builder for method chaining</returns>
	IStandaloneValidationBuilder<T> NotNull( );

	/// <summary>
	/// Adds a validation rule that the value must be null
	/// </summary>
	/// <returns>The validation builder for method chaining</returns>
	IStandaloneValidationBuilder<T> Null( );

	/// <summary>
	/// Adds a custom validation rule using a predicate function
	/// </summary>
	/// <param name="predicate">Function that returns true if the value is valid</param>
	/// <returns>The validation builder for method chaining</returns>
	IStandaloneValidationBuilder<T> Respect( Func<T, bool> predicate );

	/// <summary>
	/// Adds a custom async validation rule using a predicate function with service provider access
	/// </summary>
	/// <param name="predicate">Async function that returns true if the value is valid</param>
	/// <returns>The validation builder for method chaining</returns>
	IStandaloneValidationBuilder<T> RespectAsync( Func<T, CancellationToken, IServiceProvider?, Task<bool>> predicate );

	/// <summary>
	/// Sets a custom error message for the last added validation rule
	/// </summary>
	/// <param name="message">The error message to use when validation fails</param>
	/// <returns>The validation builder for method chaining</returns>
	IStandaloneValidationBuilder<T> WithMessage( string message );

	/// <summary>
	/// Sets a custom error code for the last added validation rule
	/// </summary>
	/// <param name="code">The error code to use when validation fails</param>
	/// <returns>The validation builder for method chaining</returns>
	IStandaloneValidationBuilder<T> WithCode( string code );

	/// <summary>
	/// Sets a custom HTTP status code for the last added validation rule
	/// </summary>
	/// <param name="statusCode">The HTTP status code to use when validation fails</param>
	/// <returns>The validation builder for method chaining</returns>
	IStandaloneValidationBuilder<T> WithStatusCode( int statusCode );

	/// <summary>
	/// Adds validation options to the last added rule for enum/constant validation using automatic option generation
	/// </summary>
	/// <returns>The validation builder for method chaining</returns>
	IStandaloneValidationBuilder<T> WithOptions( );

	/// <summary>
	/// Adds validation options to the last added rule for enum/constant validation using automatic option generation
	/// </summary>
	/// <param name="optionsType">Format type for options display</param>
	/// <returns>The validation builder for method chaining</returns>
	IStandaloneValidationBuilder<T> WithOptions( Myth.Enums.OptionsType optionsType );

	/// <summary>
	/// Adds validation options to the last added rule for enum/constant validation using manual options list
	/// </summary>
	/// <param name="options">List of valid options to display in error response</param>
	/// <returns>The validation builder for method chaining</returns>
	IStandaloneValidationBuilder<T> WithOptions( IReadOnlyList<string> options );

	/// <summary>
	/// Adds validation options to the last added rule for enum/constant validation using manual options array
	/// </summary>
	/// <param name="options">Array of valid options to display in error response</param>
	/// <returns>The validation builder for method chaining</returns>
	IStandaloneValidationBuilder<T> WithOptions( params string[ ] options );

	/// <summary>
	/// Adds a conditional rule that only applies when the given condition is true
	/// </summary>
	/// <param name="condition">Function that returns true if the rule should be applied</param>
	/// <returns>The validation builder for method chaining</returns>
	IStandaloneValidationBuilder<T> When( Func<T, bool> condition );

	/// <summary>
	/// Adds a conditional rule that only applies when the given condition is false
	/// </summary>
	/// <param name="condition">Function that returns true if the rule should NOT be applied</param>
	/// <returns>The validation builder for method chaining</returns>
	IStandaloneValidationBuilder<T> Unless( Func<T, bool> condition );

	/// <summary>
	/// Executes all validation rules and returns a result indicating success or failure
	/// </summary>
	/// <param name="serviceProvider">Optional service provider for async validation rules</param>
	/// <param name="cancellationToken">Cancellation token for async operations</param>
	/// <returns>A validation result containing success status and any validation errors</returns>
	new Task<StandaloneValidationResult> ValidateAsync( IServiceProvider? serviceProvider = null, CancellationToken cancellationToken = default );

	/// <summary>
	/// Executes all validation rules and throws ValidationException if validation fails
	/// </summary>
	/// <param name="serviceProvider">Optional service provider for async validation rules</param>
	/// <param name="cancellationToken">Cancellation token for async operations</param>
	/// <exception cref="ValidationException">Thrown when validation fails</exception>
	Task ValidateAndThrowAsync( IServiceProvider? serviceProvider = null, CancellationToken cancellationToken = default );
}
