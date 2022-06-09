using System.Net.Http.Headers;
using System.Text;
using static Myth.Extensions.JsonExtensions;

namespace Myth.Rest {

    public class RestConfigBuilder {
        protected internal string _baseUrl;
        protected internal AuthenticationHeaderValue _authorizationHeader;
        protected internal string _acceptContentType;
        protected internal CaseStrategy _serializationCaseStrategy;
        protected internal CaseStrategy _deserializationCaseStrategy;
        protected internal IDictionary<string, string> _customHeaders;

        public RestConfigBuilder( ) {
            _customHeaders = new Dictionary<string, string>( );
            _serializationCaseStrategy = CaseStrategy.CamelCase;
            _deserializationCaseStrategy = CaseStrategy.CamelCase;
        }

        public bool AuthorizationIsSetted => _authorizationHeader != null;

        public RestConfigBuilder WithBaseUrl( string baseUrl ) {
            _baseUrl = baseUrl;
            return this;
        }

        public RestConfigBuilder WithBearerAuthorization( string token ) {
            _authorizationHeader = new AuthenticationHeaderValue( "Bearer", token );
            return this;
        }

        public RestConfigBuilder WithBasicAuthorization( string username, string password ) {
            var encodedtoken = Convert.ToBase64String( Encoding.ASCII.GetBytes( $"{username}:{password}" ) );
            return this.WithBasicAuthorization( encodedtoken );
        }

        public RestConfigBuilder WithBasicAuthorization( string encodedToken ) {
            _authorizationHeader = new AuthenticationHeaderValue(
                "Basic",
                encodedToken
            );
            return this;
        }

        public RestConfigBuilder WithContentType( string contentType ) {
            _acceptContentType = contentType;
            return this;
        }

        public RestConfigBuilder AddHeader( string key, string value ) {
            _customHeaders.TryAdd( key, value );
            return this;
        }

        public RestConfigBuilder WithBodySerialization( CaseStrategy serializationCaseStrategy ) {
            _serializationCaseStrategy = serializationCaseStrategy;
            return this;
        }

        public RestConfigBuilder WithBodyDeserialization( CaseStrategy deserializationCaseStrategy ) {
            _deserializationCaseStrategy = deserializationCaseStrategy;
            return this;
        }
    }
}