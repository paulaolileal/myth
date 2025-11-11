using Myth.Builder;
using Myth.Guard;

namespace Myth.Interfaces;

/// <summary>
/// Defines a validatable entity
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public interface IValidatable<T> where T : class {

	/// <summary>
	/// Configures validation rules for the entity
	/// </summary>
	/// <param name="builder">The validation builder</param>
	/// <param name="context">The validation context</param>
	void Validate( ValidationBuilder<T> builder, ValidationContextKey? context = null );
}
