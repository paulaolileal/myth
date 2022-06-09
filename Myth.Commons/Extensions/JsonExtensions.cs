using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text;

namespace Myth.Extensions {

    public static class JsonExtensions {

        public enum CaseStrategy { CamelCase, SnakeCase }

        public static string ToJson( this object content, bool ignoreNullValue = true, CaseStrategy caseStrategy = CaseStrategy.CamelCase, Action<JsonSerializerSettings>? settings = null ) {
            var contractResolver = new DefaultContractResolver {
                NamingStrategy = StrategyResolver( caseStrategy )
            };

            var jsonSettings = new JsonSerializerSettings {
                Formatting = Formatting.Indented,
                NullValueHandling = ignoreNullValue ? NullValueHandling.Ignore : NullValueHandling.Include,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = contractResolver
            };

            if ( settings is not null )
                settings.Invoke( jsonSettings );

            try {
                return JsonConvert.SerializeObject( content, jsonSettings );
            } catch ( Exception exception ) {
                throw new JsonException( "Error on serialize object.", exception );
            }
        }

        public static object? FromJson( this string content, Type responseType, CaseStrategy caseStrategy = CaseStrategy.CamelCase, bool ignoreNullValue = true, Action<JsonSerializerSettings>? settings = null ) {
            var contractResolver = new DefaultContractResolver {
                NamingStrategy = StrategyResolver( caseStrategy )
            };

            var jsonSettings = new JsonSerializerSettings {
                Formatting = Formatting.Indented,
                NullValueHandling = ignoreNullValue ? NullValueHandling.Ignore : NullValueHandling.Include,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = contractResolver
            };

            if ( settings is not null )
                settings.Invoke( jsonSettings );

            try {
                return JsonConvert.DeserializeObject( content, responseType, jsonSettings );
            } catch ( Exception exception ) {
                throw new JsonException( "Error on deserialize object.", exception );
            }
        }

        public static TResponse? FromJson<TResponse>( this string content, CaseStrategy caseStrategy = CaseStrategy.CamelCase, bool ignoreNullValue = true, Action<JsonSerializerSettings>? settings = null ) {
            return (TResponse?) content.FromJson( typeof( TResponse ), caseStrategy, ignoreNullValue, settings );
        }

        public static HttpContent ToHttpContent( this object content, CaseStrategy caseStrategy = CaseStrategy.CamelCase ) {
            return new StringContent( content.ToJson( caseStrategy: caseStrategy ), Encoding.UTF8, "application/json" );
        }

        private static NamingStrategy StrategyResolver( CaseStrategy caseStrategy ) {
            return caseStrategy switch {
                CaseStrategy.SnakeCase => new SnakeCaseNamingStrategy( ),
                _ => new CamelCaseNamingStrategy( )
            };
        }
    }
}