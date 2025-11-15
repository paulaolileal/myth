using System.Text.Json;
using System.Text.Json.Serialization;
using Myth.Constants;
using Myth.Exceptions;
using Myth.Models;

namespace Myth.Extensions;

public static class JsonExtensions {
	private static JsonSettings _globalSettings = new( );

	/// <summary>
	/// Checks if a string represents valid JSON content
	/// </summary>
	/// <param name="content">The content to check</param>
	/// <returns>True if the content appears to be valid JSON, false otherwise</returns>
	public static bool IsValidJson( this string content ) {
		if ( string.IsNullOrWhiteSpace( content ) )
			return false;

		var trimmed = content.Trim( );

		try {
			using var doc = JsonDocument.Parse( trimmed );
			return true;
		} catch {
			return false;
		}
	}

	/// <summary>
	/// Safely deserializes JSON content, returning null for non-JSON content
	/// </summary>
	/// <typeparam name="T">The target type</typeparam>
	/// <param name="content">The JSON content</param>
	/// <param name="settings">Optional JSON settings</param>
	/// <returns>The deserialized object, or null if content is not valid JSON</returns>
	public static T? SafeFromJson<T>( this string content, Action<JsonSettings>? settings = null ) {
		try {
			if ( !content.IsValidJson( ) )
				return default( T );

			return content.FromJson<T>( settings );
		} catch {
			return default( T );
		}
	}

	/// <summary>
	/// Deserializes JSON content, throwing InvalidJsonResponseException for non-JSON content
	/// </summary>
	/// <typeparam name="T">The target type</typeparam>
	/// <param name="content">The JSON content</param>
	/// <param name="statusCode">The HTTP status code (for exception context)</param>
	/// <param name="contentType">The content type (for exception context)</param>
	/// <param name="settings">Optional JSON settings</param>
	/// <returns>The deserialized object</returns>
	/// <exception cref="InvalidJsonResponseException">Thrown when content is not valid JSON</exception>
	public static T FromJsonOrThrow<T>( this string content, System.Net.HttpStatusCode statusCode, string? contentType = null, Action<JsonSettings>? settings = null ) {
		try {
			// Handle empty content for successful responses - return default for dynamic, throw for specific types
			if ( string.IsNullOrEmpty( content ) ) {
				if ( typeof( T ) == typeof( object ) || typeof( T ).Name == "Object" ) {
					return ( T )( object )new { }; // Return empty object for dynamic
				}

				// For empty content, only throw if it's clearly not expected (not success codes like 204, and not json content type)
				if ( !IsSuccessWithEmptyContentExpected( statusCode, contentType ) ) {
					throw new InvalidJsonResponseException( statusCode, content, contentType );
				}

				return default( T )!;
			}

			if ( !content.IsValidJson( ) ) {
				throw new InvalidJsonResponseException( statusCode, content, contentType );
			}

			return content.FromJson<T>( settings )!;
		} catch ( InvalidJsonResponseException ) {
			throw; // Re-throw our custom exception
		} catch ( Exception ex ) {
			throw new InvalidJsonResponseException( statusCode, content, contentType, ex );
		}
	}

	/// <summary>
	/// Checks if empty content is expected for the given status code and content type
	/// </summary>
	private static bool IsSuccessWithEmptyContentExpected( System.Net.HttpStatusCode statusCode, string? contentType ) {
		// Success status codes where empty content is normal
		var emptyContentSuccessCodes = new[ ] {
			System.Net.HttpStatusCode.NoContent,          // 204
			System.Net.HttpStatusCode.NotModified,        // 304
			System.Net.HttpStatusCode.ResetContent        // 205
		};

		return emptyContentSuccessCodes.Contains( statusCode );
	}

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
