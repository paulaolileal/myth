using Myth.Interfaces;
using Myth.Rules.Generics;

namespace Myth.Rules.Base;

/// <summary>
/// Fluent rule builder that maintains state for chaining operations like WithMessage, WithCode, When, Unless
/// </summary>
/// <typeparam name="T">The type being validated</typeparam>
public sealed class FluentRuleBuilder<T> {
	private readonly FieldRuleBuilder<T> _fieldBuilder;
	private ValidationRuleBase<T>? _lastRule;

	public FluentRuleBuilder( FieldRuleBuilder<T> fieldBuilder ) {
		_fieldBuilder = fieldBuilder;
	}

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

	// IRuleConfigurator<T> implementation - delegate to RuleConfigurator pattern
	public FluentRuleBuilder<T> Respect( Func<T, bool> predicate ) {
		var rule = new CustomRule<T>( predicate );
		return AddRule( rule );
	}

	public FluentRuleBuilder<T> RespectAsync( Func<T, CancellationToken, IServiceProvider, Task<bool>> predicate ) {
		var rule = new CustomRule<T>( predicate );
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
