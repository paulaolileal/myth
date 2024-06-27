using Myth.Constants;
using Myth.Exceptions;
using Myth.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Myth.Extensions;

public static class JsonExtensions {

	private static JsonSerializerSettings BaseSerializer( Action<JsonSettings>? settings = null ) {
		var jsonSettings = new JsonSettings( );

		settings?.Invoke( jsonSettings );

		var contractResolver = new DefaultContractResolver {
			NamingStrategy = StrategyResolver( jsonSettings.CaseStrategy )
		};

		var serializerSettings = new JsonSerializerSettings {
			Formatting = jsonSettings.MinifyResult ? Formatting.None : Formatting.Indented,
			NullValueHandling = jsonSettings.IgnoreNullValues ? NullValueHandling.Ignore : NullValueHandling.Include,
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
			ContractResolver = contractResolver
		};

		jsonSettings.OtherSettings?.Invoke( serializerSettings );

		return serializerSettings;
	}

	/// <summary>
	/// Produces a string in json format
	/// </summary>
	/// <param name="content">An object to be serializable</param>
	/// <param name="settings">Customizations for serialization</param>
	/// <returns>A string in json format</returns>
	/// <exception cref="JsonParsingException">Throws when object can't be serializable</exception>
	public static string ToJson( this object content, Action<JsonSettings>? settings = null ) {
		var serializerSettings = BaseSerializer( settings );

		try {
			return JsonConvert.SerializeObject( content, serializerSettings );
		} catch ( Exception exception ) {
			throw new JsonParsingException( "Error on serialize object.", exception );
		}
	}

	/// <summary>
	/// Produces an object based on json in a string
	/// </summary>
	/// <param name="content">The json</param>
	/// <param name="responseType">A type to construct the object</param>
	/// <param name="settings">Customizations for deserialization</param>
	/// <returns>An object based on json</returns>
	/// <exception cref="JsonParsingException">Throws when the string can't be parsed into a file</exception>
	public static object? FromJson( this string content, Type responseType, Action<JsonSettings>? settings = null ) {
		var serializerSettings = BaseSerializer( settings );

		try {
			return JsonConvert.DeserializeObject( content, responseType, serializerSettings );
		} catch ( Exception exception ) {
			throw new JsonParsingException( "Error on deserialize object.", exception );
		}
	}

	/// <summary>
	/// Produces an object based on json in a string
	/// </summary>
	/// <param name="content">The json</param>
	/// <param name="settings">Customizations for deserialization</param>
	/// <returns>An object based on json</returns>
	/// <exception cref="JsonParsingException">Throws when the string can't be parsed into a file</exception>
	public static TResponse? FromJson<TResponse>( this string content, Action<JsonSettings>? settings = null ) =>
		( TResponse? )content.FromJson( typeof( TResponse ), settings );

	private static NamingStrategy StrategyResolver( CaseStrategy caseStrategy ) =>
		caseStrategy switch {
			CaseStrategy.SnakeCase => new SnakeCaseNamingStrategy( ),
			_ => new CamelCaseNamingStrategy( )
		};
}