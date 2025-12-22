using System.Linq.Expressions;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Myth.Exceptions;
using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Validation;

/// <summary>
/// Implementation of standalone validation builder that reuses existing validation infrastructure
/// while providing a fluent API for validating individual values outside of model context
/// </summary>
/// <typeparam name="T">The type of value being validated</typeparam>
/// <remarks>
/// Initializes a new instance of StandaloneValidationBuilder
/// </remarks>
/// <param name="value">The value to validate</param>
/// <param name="propertyName">The property name for error context</param>
internal class StandaloneValidationBuilder<T>( T value, string propertyName ) : IStandaloneValidationBuilder<T> {

	private readonly T _value = value;
	private readonly string _propertyName = propertyName ?? "Value";
	private readonly List<ValidationRuleBase<T>> _rules = new( );
	private ValidationRuleBase<T>? _lastRule;

	/// <inheritdoc />
	public IStandaloneValidationBuilder<T> NotNull( ) {
		var rule = new NotNullRule<T>( );
		AddRule( rule );
		return this;
	}

	/// <inheritdoc />
	public IStandaloneValidationBuilder<T> Null( ) {
		var rule = new NullRule<T>( );
		AddRule( rule );
		return this;
	}

	/// <inheritdoc />
	public IStandaloneValidationBuilder<T> Respect( Func<T, bool> predicate ) {
		var rule = new PredicateRule<T>( predicate );
		AddRule( rule );
		return this;
	}

	/// <inheritdoc />
	public IStandaloneValidationBuilder<T> RespectAsync( Func<T, CancellationToken, IServiceProvider?, Task<bool>> predicate ) {
		var rule = new AsyncPredicateRule<T>( predicate );
		AddRule( rule );
		return this;
	}

	/// <inheritdoc />
	public IStandaloneValidationBuilder<T> WithMessage( string message ) {
		if ( _lastRule == null )
			throw new InvalidOperationException( "No rule to apply message to. Call a validation method first." );

		_lastRule.WithMessage( message );
		return this;
	}

	/// <inheritdoc />
	public IStandaloneValidationBuilder<T> WithStatusCode( int statusCode ) {
		if ( _lastRule == null )
			throw new InvalidOperationException( "No rule to apply status code to. Call a validation method first." );

		_lastRule.WithStatusCode( ( HttpStatusCode )statusCode );
		return this;
	}

	/// <inheritdoc />
	public IStandaloneValidationBuilder<T> WithOptions( ) {
		if ( _lastRule == null )
			throw new InvalidOperationException( "No rule to apply options to. Call a validation method first." );

		_lastRule.WithOptions( );
		return this;
	}

	/// <inheritdoc />
	public IStandaloneValidationBuilder<T> WithOptions( Myth.Enums.OptionsType optionsType ) {
		if ( _lastRule == null )
			throw new InvalidOperationException( "No rule to apply options to. Call a validation method first." );

		_lastRule.WithOptions( optionsType );
		return this;
	}

	/// <inheritdoc />
	public IStandaloneValidationBuilder<T> WithOptions( IReadOnlyList<string> options ) {
		if ( _lastRule == null )
			throw new InvalidOperationException( "No rule to apply options to. Call a validation method first." );

		_lastRule.WithOptions( options );
		return this;
	}

	/// <inheritdoc />
	public IStandaloneValidationBuilder<T> WithOptions( params string[ ] options ) {
		if ( _lastRule == null )
			throw new InvalidOperationException( "No rule to apply options to. Call a validation method first." );

		_lastRule.WithOptions( options );
		return this;
	}

	/// <inheritdoc />
	public IStandaloneValidationBuilder<T> When( Func<T, bool> condition ) {
		if ( _lastRule == null )
			throw new InvalidOperationException( "No rule to apply condition to. Call a validation method first." );

		_lastRule.When( x => condition( x ) );
		return this;
	}

	/// <inheritdoc />
	public IStandaloneValidationBuilder<T> Unless( Func<T, bool> condition ) {
		if ( _lastRule == null )
			throw new InvalidOperationException( "No rule to apply condition to. Call a validation method first." );

		_lastRule.Unless( x => condition( x ) );
		return this;
	}

	/// <inheritdoc />
	public async Task<StandaloneValidationResult> ValidateAsync( IServiceProvider? serviceProvider = null, CancellationToken cancellationToken = default ) {
		var errors = new List<ValidationError>( );

		foreach ( var rule in _rules ) {
			var context = new RuleContext<T>( _value, _propertyName, serviceProvider ?? new EmptyServiceProvider( ), cancellationToken );

			var error = await rule.ValidateAsync( context );

			if ( error != null ) {
				errors.Add( error );

				if ( rule.StopOnFailure )
					break;
			}
		}

		return new StandaloneValidationResult( errors );
	}

	/// <inheritdoc />
	public async Task ValidateAndThrowAsync( IServiceProvider? serviceProvider = null, CancellationToken cancellationToken = default ) {
		var result = await ValidateAsync( serviceProvider, cancellationToken );

		if ( result.IsInvalid ) {
			var validationResult = new ValidationResult( result.Errors.ToList( ) );
			throw new ValidationException( validationResult );
		}
	}

	/// <summary>
	/// Adds a validation rule and sets it as the last rule for method chaining
	/// </summary>
	/// <param name="rule">The validation rule to add</param>
	private void AddRule( ValidationRuleBase<T> rule ) {
		_rules.Add( rule );
		_lastRule = rule;
	}

	/// <summary>
	/// Adds a validation rule from external code (used by extension methods)
	/// </summary>
	/// <param name="rule">The validation rule to add</param>
	/// <returns>This validation builder for method chaining</returns>
	internal IStandaloneValidationBuilder<T> AddExternalRule( ValidationRuleBase<T> rule ) {
		AddRule( rule );
		return this;
	}
}

/// <summary>
/// Simple predicate-based validation rule for custom validations
/// </summary>
/// <typeparam name="T">The value type being validated</typeparam>
internal class PredicateRule<T>( Func<T, bool> predicate ) : ValidationRuleBase<T> {

	private readonly Func<T, bool> _predicate = predicate ?? throw new ArgumentNullException( nameof( predicate ) );

	protected override Task<bool> EvaluateAsync( RuleContext<T> context ) {
		return Task.FromResult( _predicate( context.Value ) );
	}

	protected override string GetDefaultMessage( T value ) {
		return $"'{value}' does not meet the validation criteria";
	}
}

/// <summary>
/// Async predicate-based validation rule for custom async validations
/// </summary>
/// <typeparam name="T">The value type being validated</typeparam>
internal class AsyncPredicateRule<T>( Func<T, CancellationToken, IServiceProvider?, Task<bool>> predicate ) : ValidationRuleBase<T> {

	private readonly Func<T, CancellationToken, IServiceProvider?, Task<bool>> _predicate = predicate ?? throw new ArgumentNullException( nameof( predicate ) );

	protected override async Task<bool> EvaluateAsync( RuleContext<T> context ) {
		return await _predicate( context.Value, context.CancellationToken, context.ServiceProvider );
	}

	protected override string GetDefaultMessage( T value ) {
		return $"'{value}' does not meet the async validation criteria";
	}
}

/// <summary>
/// Simple null validation rule
/// </summary>
/// <typeparam name="T">The value type being validated</typeparam>
internal class NotNullRule<T> : ValidationRuleBase<T> {

	protected override Task<bool> EvaluateAsync( RuleContext<T> context ) {
		return Task.FromResult( context.Value != null );
	}

	protected override string GetDefaultMessage( T value ) {
		return "Value cannot be null";
	}
}

/// <summary>
/// Simple null validation rule (value must be null)
/// </summary>
/// <typeparam name="T">The value type being validated</typeparam>
internal class NullRule<T> : ValidationRuleBase<T> {

	protected override Task<bool> EvaluateAsync( RuleContext<T> context ) {
		return Task.FromResult( context.Value == null );
	}

	protected override string GetDefaultMessage( T value ) {
		return "Value must be null";
	}
}

/// <summary>
/// Empty service provider for standalone validation when no service provider is supplied
/// </summary>
internal class EmptyServiceProvider : IServiceProvider {
	public object? GetService( Type serviceType ) {
		return null;
	}
}
