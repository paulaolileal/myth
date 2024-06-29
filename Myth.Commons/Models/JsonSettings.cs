using Myth.Constants;
using Newtonsoft.Json;

namespace Myth.Models;

public class JsonSettings {
	internal bool IgnoreNullValues { get; set; } = false;

	/// <summary>
	/// Should ignore null values on object
	/// </summary>
	public JsonSettings IgnoreNull( ) {
		IgnoreNullValues = true;

		return this;
	}

	internal CaseStrategy CaseStrategy { get; set; } = CaseStrategy.CamelCase;

	/// <summary>
	/// The case strategy to be used in serialization
	/// </summary>
	public JsonSettings UseCaseStrategy( CaseStrategy strategy ) {
		CaseStrategy = strategy;

		return this;
	}

	internal bool MinifyResult { get; set; } = false;

	/// <summary>
	/// If the result should be minified
	/// </summary>
	public JsonSettings Minify( ) {
		MinifyResult = true;

		return this;
	}

	/// <summary>
	/// Other settings on base serializer settings
	/// </summary>
	public Action<JsonSerializerSettings>? OtherSettings { get; }
}