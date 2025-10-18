using Myth.Models;

namespace Myth.Interfaces {

	/// <summary>
	/// Base interface for all validation rules
	/// </summary>
	/// <typeparam name="T">The value type being validated</typeparam>
	public interface IValidationRule<T> {

		Task<ValidationError?> ValidateAsync( RuleContext<T> context );

		bool StopOnFailure { get; }
	}
}