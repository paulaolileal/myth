namespace Myth.Constants;

/// <summary>
/// Json serialization case
/// </summary>
public enum CaseStrategy {

	/// <summary>
	/// In <c>CamelCase</c> the first letter should start with lowercase and the fist letter of each subsequent new word
	/// <example>
	/// <code>
	/// {
	///	  "myAewsomeProperty": "value"
	/// }
	/// </code>
	/// </example>
	/// </summary>
	CamelCase,

	/// <summary>
	/// In <c>SnakeCase</c> use underscore instead of spaces to separate words
	/// <example>
	/// <code>
	/// {
	///	  "my_aewsome_property": "value"
	/// }
	/// </code>
	/// </example>
	/// </summary>
	SnakeCase
}
