using Myth.Constants;
using Newtonsoft.Json;

namespace Myth.Models;

public class JsonSettings {
	public bool IgnoreNullValues { get; private set; } = false;

	public JsonSettings IgnoreNull( ) {
		IgnoreNullValues = true;

		return this;
	}

	public CaseStrategy CaseStrategy { get; private set; } = CaseStrategy.CamelCase;

	public JsonSettings SetCaseAs( CaseStrategy caseStrategy ) {
		CaseStrategy = caseStrategy;
		return this;
	}

	public bool MinifyResult { get; private set; } = false;

	public JsonSettings Minify( ) {
		MinifyResult = true;
		return this;
	}

	public Action<JsonSerializerSettings>? OtherSettings { get; }
}