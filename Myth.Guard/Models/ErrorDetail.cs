namespace Myth.Models;

/// <summary>
/// Error detail model
/// </summary>
public sealed class ErrorDetail {
	public string Field { get; set; } = string.Empty;
	public string Message { get; set; } = string.Empty;
	public string Code { get; set; } = string.Empty;
}
