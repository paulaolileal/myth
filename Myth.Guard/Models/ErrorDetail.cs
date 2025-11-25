using System.Text.Json.Serialization;

namespace Myth.Models;

/// <summary>
/// Error detail model
/// </summary>
public sealed class ErrorDetail {
	public string Field { get; set; } = string.Empty;
	public string Message { get; set; } = string.Empty;
	public string Code { get; set; } = string.Empty;

	/// <summary>
	/// List of valid options for the field (only included when validation fails for enum/constant fields)
	/// </summary>
	[JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
	public IReadOnlyList<string>? Options { get; set; }
}
