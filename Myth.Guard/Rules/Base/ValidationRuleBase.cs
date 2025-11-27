using System.Net;
using Myth.Enums;
using Myth.Interfaces;
using Myth.Models;

namespace Myth.Rules.Base;

/// <summary>
/// Abstract base class for validation rules
/// </summary>
/// <typeparam name="T">The value type</typeparam>
public abstract class ValidationRuleBase<T> : IValidationRule<T> {
	protected string? CustomMessage { get; private set; }
	protected Func<T, string>? DynamicMessage { get; private set; }
	protected string Code { get; private set; } = "VIOLATION";
	protected HttpStatusCode StatusCode { get; private set; } = HttpStatusCode.BadRequest;
	protected Func<T, bool>? Condition { get; private set; }
	protected Func<T, bool>? UnlessCondition { get; private set; }
	protected Func<object, bool>? EntityCondition { get; private set; }
	protected Func<object, bool>? EntityUnlessCondition { get; private set; }
	protected IReadOnlyList<string>? Options { get; private set; }
	protected OptionsType OptionsType { get; private set; } = OptionsType.ValueAndName;
	protected bool HasOptionsConfigured { get; private set; }

	public bool StopOnFailure { get; private set; }

	protected abstract Task<bool> EvaluateAsync( RuleContext<T> context );

	protected abstract string GetDefaultMessage( T value );

	async Task<ValidationError?> IValidationRule.ValidateAsync( RuleContext<object> context ) {
		// Create typed context from object context
		var typedContext = new RuleContext<T>(
			( T )context.Value,
			context.FieldName,
			context.ServiceProvider,
			context.CancellationToken,
			context.Entity
		);
		return await ValidateAsync( typedContext );
	}

	public async Task<ValidationError?> ValidateAsync( RuleContext<T> context ) {
		if ( Condition != null && !Condition( context.Value ) )
			return null;

		if ( UnlessCondition != null && UnlessCondition( context.Value ) )
			return null;

		if ( EntityCondition != null && context.Entity != null && !EntityCondition( context.Entity ) )
			return null;

		if ( EntityUnlessCondition != null && context.Entity != null && EntityUnlessCondition( context.Entity ) )
			return null;

		var isValid = await EvaluateAsync( context );

		if ( isValid )
			return null;

		var message = DynamicMessage?.Invoke( context.Value )
			?? CustomMessage
			?? GetDefaultMessage( context.Value );

		var options = GetOptionsForError( context.Value );

		return new ValidationError {
			Field = context.FieldName,
			Message = message,
			Code = Code,
			StatusCode = StatusCode,
			Options = options
		};
	}

	public ValidationRuleBase<T> WithMessage( string message ) {
		CustomMessage = message;
		return this;
	}

	public ValidationRuleBase<T> WithMessage( Func<T, string> messageFunc ) {
		DynamicMessage = messageFunc;
		return this;
	}

	public ValidationRuleBase<T> WithCode( string code ) {
		Code = code;
		return this;
	}

	public ValidationRuleBase<T> WithStatusCode( HttpStatusCode statusCode ) {
		StatusCode = statusCode;
		return this;
	}

	public ValidationRuleBase<T> SetStopOnFailure( ) {
		StopOnFailure = true;
		return this;
	}

	public ValidationRuleBase<T> When( Func<T, bool> condition ) {
		Condition = condition;
		return this;
	}

	public ValidationRuleBase<T> Unless( Func<T, bool> condition ) {
		UnlessCondition = condition;
		return this;
	}

	public ValidationRuleBase<T> WhenEntity( Func<object, bool> condition ) {
		EntityCondition = condition;
		return this;
	}

	public ValidationRuleBase<T> UnlessEntity( Func<object, bool> condition ) {
		EntityUnlessCondition = condition;
		return this;
	}

	/// <summary>
	/// Configures validation to include options in error response using manual options list
	/// </summary>
	/// <param name="options">List of valid options to include in error response</param>
	/// <returns>The rule instance for method chaining</returns>
	public ValidationRuleBase<T> WithOptions( IReadOnlyList<string> options ) {
		Options = options;
		HasOptionsConfigured = true;
		return this;
	}

	/// <summary>
	/// Configures validation to include options in error response using automatic option generation
	/// </summary>
	/// <param name="optionsType">Format type for options display</param>
	/// <returns>The rule instance for method chaining</returns>
	public ValidationRuleBase<T> WithOptions( OptionsType optionsType = OptionsType.ValueAndName ) {
		OptionsType = optionsType;
		HasOptionsConfigured = true;
		return this;
	}

	/// <summary>
	/// Configures validation to include options in error response using manual options array
	/// </summary>
	/// <param name="options">Array of valid options to include in error response</param>
	/// <returns>The rule instance for method chaining</returns>
	public ValidationRuleBase<T> WithOptions( params string[ ] options ) {
		Options = options?.ToList( ).AsReadOnly( );
		HasOptionsConfigured = true;
		return this;
	}

	/// <summary>
	/// Gets options for error response. Override in derived classes to provide automatic options
	/// </summary>
	/// <param name="value">The invalid value</param>
	/// <returns>Options list for error response, or null if no options configured</returns>
	protected virtual IReadOnlyList<string>? GetOptionsForError( T value ) {
		if ( !HasOptionsConfigured )
			return null;

		if ( Options != null )
			return Options;

		// Override in derived classes to provide automatic option generation
		return null;
	}
}
