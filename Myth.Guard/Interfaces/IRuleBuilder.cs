using Myth.Interfaces.Rules;

namespace Myth.Interfaces;

/// <summary>
/// Base interface for all rule builders providing type-safe rule configuration
/// </summary>
/// <typeparam name="T">The type being validated</typeparam>
public interface IRuleBuilder<T> : IRuleConfigurator<T> {

	/// <summary>
	/// Gets the configured validation rules
	/// </summary>
	/// <returns>List of validation rules</returns>
	IReadOnlyList<IValidationRule<T>> GetRules( );
}
