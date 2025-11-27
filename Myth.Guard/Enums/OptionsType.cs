namespace Myth.Enums;

/// <summary>
/// Defines how validation options should be formatted in error responses
/// </summary>
public enum OptionsType {
	/// <summary>
	/// Include both value and name in the format "Value: Name" (default)
	/// Example: "1: Active"
	/// </summary>
	ValueAndName = 0,

	/// <summary>
	/// Include only the value
	/// Example: "1"
	/// </summary>
	OnlyValue = 1,

	/// <summary>
	/// Include only the name
	/// Example: "Active"
	/// </summary>
	OnlyName = 2
}
