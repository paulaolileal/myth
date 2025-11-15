using System.Net;
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

		return new ValidationError {
			Field = context.FieldName,
			Message = message,
			Code = Code,
			StatusCode = StatusCode
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
}
