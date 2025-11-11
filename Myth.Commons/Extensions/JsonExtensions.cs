using System.Text.Json;
using System.Text.Json.Serialization;
using Myth.Constants;
using Myth.Exceptions;
using Myth.Models;

namespace Myth.Extensions;

public static class JsonExtensions {
	private static JsonSettings _globalSettings = new( );

	private static JsonSerializerOptions BaseSerializer( Action<JsonSettings>? settings = null ) {
		var jsonSettings = _globalSettings.Copy( );

		settings?.Invoke( jsonSettings );

		var options = new JsonSerializerOptions {
			WriteIndented = !jsonSettings.MinifyResult,
			DefaultIgnoreCondition = jsonSettings.IgnoreNullValues
				? JsonIgnoreCondition.WhenWritingNull
				: JsonIgnoreCondition.Never,
			PropertyNamingPolicy = StrategyResolver( jsonSettings.CaseStrategy ),
			ReferenceHandler = ReferenceHandler.IgnoreCycles
		};

		// Adicionar conversores customizados
		foreach ( var converter in jsonSettings.Converters ) {
			options.Converters.Add( converter );
		}

		jsonSettings.OtherSettings?.Invoke( options );

		return options;
	}

	/// <summary>
	/// Produces a string in json format
	/// </summary>
	/// <param name="content">An object to be serializable</param>
	/// <param name="settings">Customizations for serialization</param>
	/// <returns>A string in json format</returns>
	/// <exception cref="JsonParsingException">Throws when object can't be serializable</exception>
	public static string ToJson( this object content, Action<JsonSettings>? settings = null ) {
		var serializerOptions = BaseSerializer( settings );

		try {
			return JsonSerializer.Serialize( content, serializerOptions );
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
		var serializerOptions = BaseSerializer( settings );

		try {
			// Handle empty or whitespace content
			if ( string.IsNullOrWhiteSpace( content ) ) {
				return responseType == typeof( string ) ? content : null;
			}

			// Special handling for dynamic type to maintain compatibility with Newtonsoft.Json behavior
			if ( responseType == typeof( object ) || responseType.Name == "Object" ) {
				var jsonElement = JsonSerializer.Deserialize<JsonElement>( content, serializerOptions );
				return ConvertJsonElementToDynamic( jsonElement );
			}

			return JsonSerializer.Deserialize( content, responseType, serializerOptions );
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

	private static JsonNamingPolicy? StrategyResolver( CaseStrategy caseStrategy ) =>
		caseStrategy switch {
			CaseStrategy.SnakeCase => JsonNamingPolicy.SnakeCaseLower,
			_ => JsonNamingPolicy.CamelCase
		};

	/// <summary>
	/// Set configurations of json in a globally form
	/// </summary>
	/// <param name="settings"></param>
	public static void Configure( Action<JsonSettings>? settings ) => settings?.Invoke( _globalSettings );

	private static dynamic ConvertJsonElementToDynamic( JsonElement element ) {
		switch ( element.ValueKind ) {
			case JsonValueKind.Object:
				var expandoObject = new System.Dynamic.ExpandoObject( );
				var dictionary = ( IDictionary<string, object?> )expandoObject;
				foreach ( var property in element.EnumerateObject( ) ) {
					dictionary[ property.Name ] = ConvertJsonElementToDynamic( property.Value );
				}
				return expandoObject;

			case JsonValueKind.Array:
				return element.EnumerateArray( ).Select( ConvertJsonElementToDynamic ).ToArray( );

			case JsonValueKind.String:
				return element.GetString( )!;

			case JsonValueKind.Number:
				if ( element.TryGetInt32( out var intValue ) )
					return intValue;
				if ( element.TryGetInt64( out var longValue ) )
					return longValue;
				return element.GetDouble( );

			case JsonValueKind.True:
				return true;

			case JsonValueKind.False:
				return false;

			case JsonValueKind.Null:
				return null!;

			default:
				throw new ArgumentOutOfRangeException( nameof( element.ValueKind ), element.ValueKind, "Unsupported JsonValueKind" );
		}
	}
}
