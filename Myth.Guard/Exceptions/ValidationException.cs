using Myth.Models;

namespace Myth.Exceptions {

	/// <summary>
	/// Exception thrown when validation fails
	/// </summary>
	public sealed class ValidationException : Exception {
		public ValidationResult ValidationResult { get; }

		public ValidationException( ValidationResult result )
			: base( "Validation failed" + ( result is not null && !result.IsValid ? $" {result.Errors.Count} error(s)" : string.Empty ) ) {
			ArgumentNullException.ThrowIfNull( result, nameof( ValidationResult ) );
			ValidationResult = result;
		}
	}
}