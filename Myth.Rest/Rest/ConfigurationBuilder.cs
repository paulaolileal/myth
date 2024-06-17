using Myth.Constants;
using System.Net.Http.Headers;
using System.Text;

namespace Myth.Rest {

	public class ConfigurationBuilder {
		protected internal string? _baseUrl;
		protected internal TimeSpan? _timeout;
		protected internal AuthenticationHeaderValue? _authorizationHeader;
		protected internal string? _acceptableContentType;
		protected internal CaseStrategy _serializationCaseStrategy;
		protected internal CaseStrategy _deserializationCaseStrategy;
		protected internal IDictionary<string, string> _customHeaders;
		protected internal HttpClient _httpClient;

		public ConfigurationBuilder( ) {
			_httpClient = new HttpClient( );
			_customHeaders = new Dictionary<string, string>( );
			_serializationCaseStrategy = CaseStrategy.CamelCase;
			_deserializationCaseStrategy = CaseStrategy.CamelCase;
		}

		public ConfigurationBuilder WithClient( HttpClient httpClient ) {
			_httpClient = httpClient;
			return this;
		}

		public ConfigurationBuilder WithBaseUrl( string baseUrl ) {
			_baseUrl = baseUrl;
			return this;
		}

		public ConfigurationBuilder WithTimeout( TimeSpan timeout ) {
			_timeout = timeout;
			return this;
		}

		public ConfigurationBuilder WithAuthorization( string scheme, string token ) {
			_authorizationHeader = new AuthenticationHeaderValue( scheme, token );
			return this;
		}

		public ConfigurationBuilder WithBearerAuthorization( string token ) {
			WithAuthorization( "Bearer", token );
			return this;
		}

		public ConfigurationBuilder WithBasicAuthorization( string username, string password ) {
			var encodedtoken = Convert.ToBase64String( Encoding.ASCII.GetBytes( $"{username}:{password}" ) );

			return WithBasicAuthorization( encodedtoken );
		}

		public ConfigurationBuilder WithBasicAuthorization( string encodedToken ) {
			WithAuthorization( "Basic", encodedToken );

			return this;
		}

		public ConfigurationBuilder WithContentType( string contentType ) {
			_acceptableContentType = contentType;
			return this;
		}

		public ConfigurationBuilder AddHeader( string key, string value, bool replaceIfExists = true ) {
			if ( _customHeaders.ContainsKey( key ) )
				_customHeaders.Remove( key );
			_customHeaders.TryAdd( key, value );
			return this;
		}

		public ConfigurationBuilder WithBodySerialization( CaseStrategy serializationCaseStrategy ) {
			_serializationCaseStrategy = serializationCaseStrategy;
			return this;
		}

		public ConfigurationBuilder WithBodyDeserialization( CaseStrategy deserializationCaseStrategy ) {
			_deserializationCaseStrategy = deserializationCaseStrategy;
			return this;
		}
	}
}