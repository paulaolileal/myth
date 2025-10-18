using Myth.Interfaces;

namespace Myth.Builder {

	/// <summary>
	/// Represents field validation configuration
	/// </summary>
	internal sealed class FieldValidation {
		public string FieldName { get; init; } = string.Empty;
		public List<IValidationRule<object>> Rules { get; init; } = new( );
	}
}