using Myth.Constants;
using System.Net.Http.Headers;
using System.Text;

namespace Myth.Rest;

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

	/// <summary>
	/// Set a pre-built http client
	/// </summary>
	/// <param name="httpClient">Http client</param>
	/// <returns>This object</returns>
	public ConfigurationBuilder WithClient( HttpClient httpClient ) {
		_httpClient = httpClient;
		return this;
	}

	/// <summary>
	/// Set a base url for request
	/// </summary>
	/// <param name="baseUrl">A base url</param>
	/// <returns>This object</returns>
	public ConfigurationBuilder WithBaseUrl( string baseUrl ) {
		_baseUrl = baseUrl;
		return this;
	}

	/// <summary>
	/// Set a timeout until aborts the request
	/// </summary>
	/// <param name="timeout">The timeout</param>
	/// <returns>This object</returns>
	public ConfigurationBuilder WithTimeout( TimeSpan timeout ) {
		_timeout = timeout;
		return this;
	}

	/// <summary>
	/// Set a authorization header
	/// </summary>
	/// <param name="scheme">Authorization schema</param>
	/// <param name="token">The token</param>
	/// <returns>This object</returns>
	public ConfigurationBuilder WithAuthorization( string scheme, string token ) {
		_authorizationHeader = new AuthenticationHeaderValue( scheme, token );
		return this;
	}

	/// <summary>
	/// Set a bearer authorization header
	/// </summary>
	/// <param name="token">The token</param>
	/// <returns>This object</returns>
	/// <remarks>
	/// The word `Bearer` doesn't need to be ironed
	/// </remarks>
	public ConfigurationBuilder WithBearerAuthorization( string token ) {
		WithAuthorization( "Bearer", token );
		return this;
	}

	/// <summary>
	/// Set a basic authorization header
	/// </summary>
	/// <param name="username">The username to be encoded</param>
	/// <param name="password">The password to be encoded</param>
	/// <returns>This object</returns>
	/// <remarks>
	/// The word `Basic` doesn't need to be ironed
	/// </remarks>
	public ConfigurationBuilder WithBasicAuthorization( string username, string password ) {
		var encodedtoken = Convert.ToBase64String( Encoding.ASCII.GetBytes( $"{username}:{password}" ) );

		return WithBasicAuthorization( encodedtoken );
	}

	/// <summary>
	/// Set a basic authorization header
	/// </summary>
	/// <param name="encodedToken">The token</param>
	/// <returns>This object</returns>
	/// <remarks>
	/// The word `Basic` doesn't need to be ironed
	/// </remarks>
	public ConfigurationBuilder WithBasicAuthorization( string encodedToken ) {
		WithAuthorization( "Basic", encodedToken );

		return this;
	}

	/// <summary>
	/// Set a acceptable content type
	/// </summary>
	/// <param name="contentType">The content type</param>
	/// <returns>This object</returns>
	public ConfigurationBuilder WithContentType( string contentType ) {
		_acceptableContentType = contentType;
		return this;
	}

	/// <summary>
	/// Add a header
	/// </summary>
	/// <param name="key">Header key</param>
	/// <param name="value">Header value</param>
	/// <returns>This object</returns>
	public ConfigurationBuilder AddHeader( string key, string value ) {
		if ( _customHeaders.ContainsKey( key ) )
			_customHeaders.Remove( key );

		_customHeaders.TryAdd( key, value );
		return this;
	}

	/// <summary>
	/// Set the serialization of request body
	/// </summary>
	/// <param name="serializationCaseStrategy">The case of strategy</param>
	/// <returns>This object</returns>
	public ConfigurationBuilder WithBodySerialization( CaseStrategy serializationCaseStrategy ) {
		_serializationCaseStrategy = serializationCaseStrategy;
		return this;
	}

	/// <summary>
	/// Set the serialization of response body
	/// </summary>
	/// <param name="deserializationCaseStrategy">The case of strategy</param>
	/// <returns>This object</returns>
	public ConfigurationBuilder WithBodyDeserialization( CaseStrategy deserializationCaseStrategy ) {
		_deserializationCaseStrategy = deserializationCaseStrategy;
		return this;
	}
}