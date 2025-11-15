using Myth.Models;

namespace Myth.Interfaces;

/// <summary>
/// Non-generic base interface for validation rules
/// </summary>
public interface IValidationRule {

	Task<ValidationError?> ValidateAsync( RuleContext<object> context );

	bool StopOnFailure { get; }
}

/// <summary>
/// Typed interface for validation rules
/// </summary>
/// <typeparam name="T">The value type being validated</typeparam>
public interface IValidationRule<T> : IValidationRule {

	new Task<ValidationError?> ValidateAsync( RuleContext<T> context );
}
