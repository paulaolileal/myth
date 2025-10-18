namespace Myth.Models {

	/// <summary>
	/// Validation error response model
	/// </summary>
	internal sealed class ValidationErrorResponse {
		public string Code { get; set; } = string.Empty;
		public List<ErrorDetail> Errors { get; set; } = new( );
	}
}