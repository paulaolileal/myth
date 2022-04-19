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