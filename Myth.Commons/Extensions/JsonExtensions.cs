using Myth.Constants;
using Myth.Exceptions;
using Myth.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Myth.Extensions {

	public static partial class JsonExtensions {

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

		public static string ToJson( this object content, Action<JsonSettings>? settings = null ) {
			var serializerSettings = BaseSerializer( settings );

			try {
				return JsonConvert.SerializeObject( content, serializerSettings );
			} catch ( Exception exception ) {
				throw new JsonParsingException( "Error on serialize object.", exception );
			}
		}

		public static object? FromJson( this string content, Type responseType, Action<JsonSettings>? settings = null ) {
			var serializerSettings = BaseSerializer( settings );

			try {
				return JsonConvert.DeserializeObject( content, responseType, serializerSettings );
			} catch ( Exception exception ) {
				throw new JsonParsingException( "Error on deserialize object.", exception );
			}
		}

		public static TResponse? FromJson<TResponse>( this string content, Action<JsonSettings>? settings = null ) {
			return ( TResponse? )content.FromJson( typeof( TResponse ), settings );
		}

		private static NamingStrategy StrategyResolver( CaseStrategy caseStrategy ) {
			return caseStrategy switch {
				CaseStrategy.SnakeCase => new SnakeCaseNamingStrategy( ),
				_ => new CamelCaseNamingStrategy( )
			};
		}
	}
}