using Myth.Enums;
using Myth.Interfaces;
using Myth.Rules.Generics;

namespace Myth.Rules.Base;

/// <summary>
/// Fluent rule builder that maintains state for chaining operations like WithMessage, WithCode, When, Unless
/// </summary>
/// <typeparam name="T">The type being validated</typeparam>
public sealed class FluentRuleBuilder<T>( FieldRuleBuilder<T> fieldBuilder ) {
	private readonly FieldRuleBuilder<T> _fieldBuilder = fieldBuilder;
	private ValidationRuleBase<T>? _lastRule;

	/// <summary>
	/// Gets the rules from the underlying field builder
	/// </summary>
	public IReadOnlyList<IValidationRule<T>> GetRules( ) {
		return _fieldBuilder.GetRules( );
	}

	/// <summary>
	/// Adds a rule and keeps reference for chaining
	/// </summary>
	public FluentRuleBuilder<T> AddRule( ValidationRuleBase<T> rule ) {
		_lastRule = rule;
		_fieldBuilder.AddRule( rule );
		return this;
	}

	/// <summary>
	/// Sets custom message on the last added rule
	/// </summary>
	public FluentRuleBuilder<T> WithMessage( string message ) {
		_lastRule?.WithMessage( message );
		return this;
	}

	/// <summary>
	/// Sets custom message with dynamic function on the last added rule
	/// </summary>
	public FluentRuleBuilder<T> WithMessage( Func<T, string> messageFunc ) {
		_lastRule?.WithMessage( messageFunc );
		return this;
	}

	/// <summary>
	/// Sets custom code on the last added rule
	/// </summary>
	public FluentRuleBuilder<T> WithCode( string code ) {
		_lastRule?.WithCode( code );
		return this;
	}

	/// <summary>
	/// Sets custom status code on the last added rule
	/// </summary>
	public FluentRuleBuilder<T> WithStatusCode( System.Net.HttpStatusCode statusCode ) {
		_lastRule?.WithStatusCode( statusCode );
		return this;
	}

	/// <summary>
	/// Sets stop on failure on the last added rule
	/// </summary>
	public FluentRuleBuilder<T> SetStopOnFailure( ) {
		_lastRule?.SetStopOnFailure( );
		return this;
	}

	/// <summary>
	/// Adds When condition to the last added rule
	/// </summary>
	public FluentRuleBuilder<T> When( Func<T, bool> condition ) {
		_lastRule?.When( condition );
		return this;
	}

	/// <summary>
	/// Adds Unless condition to the last added rule
	/// </summary>
	public FluentRuleBuilder<T> Unless( Func<T, bool> condition ) {
		_lastRule?.Unless( condition );
		return this;
	}

	/// <summary>
	/// Adds When condition at entity level to the last added rule
	/// </summary>
	public FluentRuleBuilder<T> WhenEntity( Func<object, bool> condition ) {
		_lastRule?.WhenEntity( condition );
		return this;
	}

	/// <summary>
	/// Adds Unless condition at entity level to the last added rule
	/// </summary>
	public FluentRuleBuilder<T> UnlessEntity( Func<object, bool> condition ) {
		_lastRule?.UnlessEntity( condition );
		return this;
	}

	/// <summary>
	/// Configures validation to include options in error response using manual options list
	/// </summary>
	/// <param name="options">List of valid options to include in error response</param>
	/// <returns>The fluent rule builder for method chaining</returns>
	public FluentRuleBuilder<T> WithOptions( IReadOnlyList<string> options ) {
		_lastRule?.WithOptions( options );
		return this;
	}

	/// <summary>
	/// Configures validation to include options in error response using automatic option generation
	/// </summary>
	/// <param name="optionsType">Format type for options display</param>
	/// <returns>The fluent rule builder for method chaining</returns>
	public FluentRuleBuilder<T> WithOptions( OptionsType optionsType = OptionsType.ValueAndName ) {
		_lastRule?.WithOptions( optionsType );
		return this;
	}

	/// <summary>
	/// Configures validation to include options in error response using manual options array
	/// </summary>
	/// <param name="options">Array of valid options to include in error response</param>
	/// <returns>The fluent rule builder for method chaining</returns>
	public FluentRuleBuilder<T> WithOptions( params string[ ] options ) {
		_lastRule?.WithOptions( options );
		return this;
	}

	// IRuleConfigurator<T> implementation - delegate to RuleConfigurator pattern
	public FluentRuleBuilder<T> Respect( Func<T, bool> predicate ) {
		var rule = new CustomRule<T>( predicate );
		return AddRule( rule );
	}

	public FluentRuleBuilder<T> RespectAsync( Func<T, CancellationToken, IServiceProvider, Task<bool>> predicate ) {
		var rule = new CustomRule<T>( predicate );
		return AddRule( rule );
	}

	/// <summary>
	/// Validates the property value using a custom predicate that also has access to the entire entity being validated
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity being validated</typeparam>
	/// <param name="predicate">A function that receives the property value and the parent entity, and returns true if valid</param>
	/// <returns>The fluent rule builder for method chaining</returns>
	/// <example>
	/// <code>
	/// // Validate email considering user's status
	/// builder.For(Email, x => x
	///     .Respect&lt;User&gt;((email, user) => user.IsActive || string.IsNullOrEmpty(email))
	///     .WithMessage("Active users must have an email"));
	/// </code>
	/// </example>
	public FluentRuleBuilder<T> Respect<TEntity>( Func<T, TEntity, bool> predicate ) where TEntity : class {
		var entityPredicate = new Func<T, object, bool>( ( value, entity ) => predicate( value, ( TEntity )entity ) );
		var rule = new CustomRule<T>( entityPredicate );
		return AddRule( rule );
	}

	/// <summary>
	/// Asynchronously validates the property value using a custom predicate that also has access to the entire entity being validated
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity being validated</typeparam>
	/// <param name="predicate">An async function that receives the property value, parent entity, cancellation token, and service provider</param>
	/// <returns>The fluent rule builder for method chaining</returns>
	/// <example>
	/// <code>
	/// // Validate credentials by checking both email and password
	/// builder.For(Email, x => x
	///     .RespectAsync&lt;LoginDto&gt;(async (email, loginData, ct, sp) => {
	///         var userRepo = sp.GetRequiredService&lt;IUserRepository&gt;();
	///         return await userRepo.AnyAsync(u => u.Email == email && u.Password == loginData.Password, ct);
	///     })
	///     .WithMessage("Invalid credentials"));
	/// </code>
	/// </example>
	public FluentRuleBuilder<T> RespectAsync<TEntity>( Func<T, TEntity, CancellationToken, IServiceProvider, Task<bool>> predicate ) where TEntity : class {
		var entityPredicate = new Func<T, object, CancellationToken, IServiceProvider, Task<bool>>(
			( value, entity, ct, sp ) => predicate( value, ( TEntity )entity, ct, sp )
		);
		var rule = new CustomRule<T>( entityPredicate );
		return AddRule( rule );
	}

	public FluentRuleBuilder<T> EqualsTo( T value ) {
		var rule = new EqualsRule<T>( value );
		return AddRule( rule );
	}

	public FluentRuleBuilder<T> NotEqualsTo( T value ) {
		var rule = new NotEqualsRule<T>( value );
		return AddRule( rule );
	}

	public FluentRuleBuilder<T> BeNull( ) {
		var rule = new BeNullRule<T>( );
		return AddRule( rule );
	}

	public FluentRuleBuilder<T> NotNull( ) {
		var rule = new NotNullRule<T>( );
		return AddRule( rule );
	}

	public FluentRuleBuilder<T> BeDefault( ) {
		var rule = new BeDefaultRule<T>( );
		return AddRule( rule );
	}

	public FluentRuleBuilder<T> NotDefault( ) {
		var rule = new NotDefaultRule<T>( );
		return AddRule( rule );
	}
}
