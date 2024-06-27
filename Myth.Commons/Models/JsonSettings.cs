using Myth.Constants;
using Newtonsoft.Json;

namespace Myth.Models;

public class JsonSettings {

	/// <summary>
	/// Should ignore null values on object
	/// </summary>
	public bool IgnoreNullValues { get; set; } = false;

	/// <summary>
	/// The case strategy to be used in serialization
	/// </summary>
	public CaseStrategy CaseStrategy { get; set; } = CaseStrategy.CamelCase;

	/// <summary>
	/// If the result should be minified
	/// </summary>
	public bool MinifyResult { get; set; } = false;

	/// <summary>
	/// Other settings on base serializer settings
	/// </summary>
	public Action<JsonSerializerSettings>? OtherSettings { get; }
}