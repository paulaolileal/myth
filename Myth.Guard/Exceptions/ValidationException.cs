using Myth.Models;

namespace Myth.Exceptions {

	/// <summary>
	/// Exception thrown when validation fails
	/// </summary>
	public sealed class ValidationException : Exception {
		public ValidationResult ValidationResult { get; }

		public ValidationException( ValidationResult result )
			: base( "Validation failed" ) {
			ValidationResult = result;
		}
	}
}