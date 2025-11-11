using Myth.Guard;
using Myth.Models;

namespace Myth.Interfaces; 

/// <summary>
/// Provides validation services
/// </summary>
public interface IValidator {

	/// <summary>
	/// Validates an entity asynchronously
	/// </summary>
	/// <typeparam name="T">The entity type</typeparam>
	/// <param name="entity">The entity to validate</param>
	/// <param name="context">The validation context</param>
	/// <param name="cancellationToken">Cancellation token</param>
	Task ValidateAsync<T>( T entity, ValidationContextKey? context = null, CancellationToken cancellationToken = default )
		where T : class;

	/// <summary>
	/// Validates an entity and returns the result without throwing
	/// </summary>
	/// <typeparam name="T">The entity type</typeparam>
	/// <param name="entity">The entity to validate</param>
	/// <param name="context">The validation context</param>
	/// <param name="cancellationToken">Cancellation token</param>
	Task<ValidationResult> ValidateAndReturnAsync<T>( T entity, ValidationContextKey? context = null, CancellationToken cancellationToken = default )
		where T : class;
}
