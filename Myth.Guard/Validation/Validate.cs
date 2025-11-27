using Myth.Models;

namespace Myth.Validation;

/// <summary>
/// Static utility for validating multiple values simultaneously with parallel execution and aggregated results
/// </summary>
public static class Validate {

	/// <summary>
	/// Starts building a multi-validation operation that can validate multiple values in parallel
	/// </summary>
	/// <returns>A builder for configuring multiple validations</returns>
	public static IMultiValidationBuilder All( ) {
		return new MultiValidationBuilder( );
	}

	/// <summary>
	/// Validates multiple validation builders in parallel and returns aggregated results
	/// </summary>
	/// <param name="validations">Collection of validation builders to execute</param>
	/// <param name="serviceProvider">Optional service provider for async validation rules</param>
	/// <param name="cancellationToken">Cancellation token for async operations</param>
	/// <returns>Combined validation result with all errors from all validations</returns>
	public static async Task<MultiValidationResult> AllAsync(
		IEnumerable<IStandaloneValidationBuilder> validations,
		IServiceProvider? serviceProvider = null,
		CancellationToken cancellationToken = default ) {

		var validationList = validations.ToList( );

		if ( !validationList.Any( ) ) {
			return MultiValidationResult.Success( );
		}

		// Execute all validations in parallel for better performance
		var tasks = validationList.Select( validation =>
			validation.ValidateAsync( serviceProvider, cancellationToken )
		).ToArray( );

		var results = await Task.WhenAll( tasks );

		// Aggregate all errors from all validations
		var allErrors = results.SelectMany( r => r.Errors ).ToList( );

		return new MultiValidationResult( allErrors );
	}

	/// <summary>
	/// Validates multiple validation builders in parallel and throws ValidationException if any validation fails
	/// </summary>
	/// <param name="validations">Collection of validation builders to execute</param>
	/// <param name="serviceProvider">Optional service provider for async validation rules</param>
	/// <param name="cancellationToken">Cancellation token for async operations</param>
	/// <exception cref="Myth.Exceptions.ValidationException">Thrown when any validation fails</exception>
	public static async Task AllAndThrowAsync(
		IEnumerable<IStandaloneValidationBuilder> validations,
		IServiceProvider? serviceProvider = null,
		CancellationToken cancellationToken = default ) {

		var result = await AllAsync( validations, serviceProvider, cancellationToken );

		if ( result.IsInvalid ) {
			var validationResult = new ValidationResult( result.Errors.ToList( ) );
			throw new Myth.Exceptions.ValidationException( validationResult );
		}
	}
}

/// <summary>
/// Non-generic base interface for standalone validation builders to enable collection usage
/// </summary>
public interface IStandaloneValidationBuilder {
	/// <summary>
	/// Executes validation and returns result
	/// </summary>
	Task<StandaloneValidationResult> ValidateAsync( IServiceProvider? serviceProvider = null, CancellationToken cancellationToken = default );
}

/// <summary>
/// Builder interface for configuring multiple validations
/// </summary>
public interface IMultiValidationBuilder {
	/// <summary>
	/// Adds a validation builder to the collection
	/// </summary>
	/// <param name="validation">The validation builder to add</param>
	/// <returns>The multi-validation builder for method chaining</returns>
	IMultiValidationBuilder Add( IStandaloneValidationBuilder validation );

	/// <summary>
	/// Adds multiple validation builders to the collection
	/// </summary>
	/// <param name="validations">The validation builders to add</param>
	/// <returns>The multi-validation builder for method chaining</returns>
	IMultiValidationBuilder Add( params IStandaloneValidationBuilder[ ] validations );

	/// <summary>
	/// Adds multiple validation builders to the collection
	/// </summary>
	/// <param name="validations">The validation builders to add</param>
	/// <returns>The multi-validation builder for method chaining</returns>
	IMultiValidationBuilder Add( IEnumerable<IStandaloneValidationBuilder> validations );

	/// <summary>
	/// Executes all validations in parallel and returns aggregated results
	/// </summary>
	/// <param name="serviceProvider">Optional service provider for async validation rules</param>
	/// <param name="cancellationToken">Cancellation token for async operations</param>
	/// <returns>Combined validation result with all errors from all validations</returns>
	Task<MultiValidationResult> ValidateAsync( IServiceProvider? serviceProvider = null, CancellationToken cancellationToken = default );

	/// <summary>
	/// Executes all validations in parallel and throws ValidationException if any validation fails
	/// </summary>
	/// <param name="serviceProvider">Optional service provider for async validation rules</param>
	/// <param name="cancellationToken">Cancellation token for async operations</param>
	/// <exception cref="Myth.Exceptions.ValidationException">Thrown when any validation fails</exception>
	Task ValidateAndThrowAsync( IServiceProvider? serviceProvider = null, CancellationToken cancellationToken = default );
}

/// <summary>
/// Implementation of multi-validation builder
/// </summary>
internal class MultiValidationBuilder : IMultiValidationBuilder {
	private readonly List<IStandaloneValidationBuilder> _validations = new( );

	public IMultiValidationBuilder Add( IStandaloneValidationBuilder validation ) {
		if ( validation != null ) {
			_validations.Add( validation );
		}
		return this;
	}

	public IMultiValidationBuilder Add( params IStandaloneValidationBuilder[ ] validations ) {
		if ( validations != null ) {
			_validations.AddRange( validations.Where( v => v != null ) );
		}
		return this;
	}

	public IMultiValidationBuilder Add( IEnumerable<IStandaloneValidationBuilder> validations ) {
		if ( validations != null ) {
			_validations.AddRange( validations.Where( v => v != null ) );
		}
		return this;
	}

	public async Task<MultiValidationResult> ValidateAsync( IServiceProvider? serviceProvider = null, CancellationToken cancellationToken = default ) {
		return await Validate.AllAsync( _validations, serviceProvider, cancellationToken );
	}

	public async Task ValidateAndThrowAsync( IServiceProvider? serviceProvider = null, CancellationToken cancellationToken = default ) {
		await Validate.AllAndThrowAsync( _validations, serviceProvider, cancellationToken );
	}
}
