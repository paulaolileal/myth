using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;

namespace Myth.Extensions {

    public static class JsonExtensions {

        public static string ToJson( this object content, bool ignoreNullValue = true, Action<JsonSerializerSettings>? settings = null ) {
            var jsonSettings = new JsonSerializerSettings {
                Formatting = Formatting.Indented,
                NullValueHandling = ignoreNullValue ? NullValueHandling.Ignore : NullValueHandling.Include,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };
            
            if(settings is not null)
                settings.Invoke( jsonSettings );

            try {
                return JsonConvert.SerializeObject( content, jsonSettings );
            } catch ( Exception exception ) {
                throw new JsonException( "Error on serialize object.", exception );
            }
        }

        public static HttpContent ToHttpContent( this object content ) {
            return new StringContent( content.ToJson( ), Encoding.UTF8, "application/json" );
        }
    }
}