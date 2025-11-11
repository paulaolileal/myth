using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.Extensions.Logging;
using Myth.Constants;
using Myth.Helpers;
using Myth.Interfaces;
using Myth.Models;

namespace Myth.Builders;

public class ConfigurationBuilder : IDisposable {
	protected internal string? _baseUrl;
	protected internal TimeSpan? _timeout;
	protected internal AuthenticationHeaderValue? _authorizationHeader;
	protected internal string? _acceptableContentType;
	protected internal CaseStrategy _serializationCaseStrategy;
	protected internal CaseStrategy _deserializationCaseStrategy;
	protected internal IDictionary<Type, Type> _jsonConverters;
	protected internal IDictionary<string, string> _customHeaders;
	protected internal RetryPolicy _retryPolicy;
	protected internal HttpClient _httpClient;
	protected internal ILogger? _logger;
	protected internal bool _enableRequestLogging = false;
	protected internal bool _enableResponseLogging = false;
	protected internal ICircuitBreaker? _circuitBreaker;
	protected internal CertificateOptions? _certificateOptions;
	protected internal HttpClientHandler? _customHandler;

	private bool _ownsHttpClient = true;

	private static readonly HashSet<string> SensitiveHeaders = new( StringComparer.OrdinalIgnoreCase ) {
		"Authorization", "X-API-Key", "Cookie", "X-Auth-Token"
	};

	public ConfigurationBuilder( ) {
		_httpClient = new HttpClient( );
		_customHeaders = new Dictionary<string, string>( );
		_retryPolicy = new RetryPolicy( );
		_serializationCaseStrategy = CaseStrategy.CamelCase;
		_deserializationCaseStrategy = CaseStrategy.CamelCase;
		_jsonConverters = new Dictionary<Type, Type>( );
	}

	public void Dispose( ) {
		if ( _ownsHttpClient )
			_httpClient?.Dispose( );
		_customHandler?.Dispose( );
		GC.SuppressFinalize( this );
	}

	/// <summary>
	/// Set a pre-built http client
	/// </summary>
	/// <param name="httpClient">Http client</param>
	/// <returns>This object</returns>
	public ConfigurationBuilder WithClient( HttpClient httpClient ) {
		if ( _ownsHttpClient ) {
			_httpClient?.Dispose( );
			_customHandler?.Dispose( );
		}

		_httpClient = httpClient;
		_ownsHttpClient = false; // Não deve fazer dispose de cliente externo
		_customHandler = null; // Clear custom handler since we're using external client

		return this;
	}

	/// <summary>
	/// Get configured HttpClient, creating one with certificates if needed
	/// </summary>
	/// <returns>Configured HttpClient</returns>
	internal HttpClient GetConfiguredHttpClient( ) {
		if ( _certificateOptions != null && _ownsHttpClient ) {
			EnsureHttpClientWithCertificates( );
		}

		return _httpClient;
	}

	/// <summary>
	/// Ensure HttpClient is configured with certificates
	/// </summary>
	private void EnsureHttpClientWithCertificates( ) {
		if ( _certificateOptions == null || !_ownsHttpClient )
			return;

		try {
			// Create new handler with certificates
			var handler = new HttpClientHandler( );
			CertificateHelper.ConfigureHandler( handler, _certificateOptions );

			// Dispose existing resources if we own them
			if ( _ownsHttpClient ) {
				_httpClient?.Dispose( );
				_customHandler?.Dispose( );
			}

			// Create new HttpClient with configured handler
			_customHandler = handler;
			_httpClient = new HttpClient( handler );
			_ownsHttpClient = true;

			_logger?.LogDebug( "HttpClient configured with client certificate: {CertificateType}", _certificateOptions.Type );
		} catch ( Exception ex ) {
			_logger?.LogError( ex, "Failed to configure HttpClient with certificates" );
			throw;
		}
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
	public ConfigurationBuilder WithHeader( string key, string value ) {
		ArgumentException.ThrowIfNullOrWhiteSpace( key, nameof( key ) );
		ArgumentException.ThrowIfNullOrWhiteSpace( value, nameof( value ) );

		if ( _customHeaders.ContainsKey( key ) )
			_customHeaders.Remove( key );

		if ( SensitiveHeaders.Contains( key ) )
			_logger?.LogWarning( "Adding sensitive header {HeaderName}. Consider using specific authorization methods.", key );

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

	/// <summary>
	/// Set default retry (3 tries, backoff exponential with jitter)
	/// </summary>
	public ConfigurationBuilder WithRetry( ) {
		_retryPolicy = new RetryPolicy( )
			.WithMaxAttempts( 3 )
			.UseExponentialBackoffWithJitter( TimeSpan.FromSeconds( 1 ) )
			.ForServerErrors( );

		return this;
	}

	/// <summary>
	/// Set custom retry police
	/// </summary>
	public ConfigurationBuilder WithRetry( Action<RetryPolicy> configure ) {
		configure( _retryPolicy );
		return this;
	}

	/// <summary>
	/// Set basic retry
	/// </summary>
	public ConfigurationBuilder WithRetry( int amount, TimeSpan timeBetweenRetries, params HttpStatusCode[ ] statusCodes ) {
		_retryPolicy.Set( amount, timeBetweenRetries, statusCodes );
		return this;
	}

	/// <summary>
	/// Add a custom converter for type on deserialization of response
	/// </summary>
	/// <typeparam name="TInterface"></typeparam>
	/// <typeparam name="TType"></typeparam>
	/// <returns></returns>
	public ConfigurationBuilder WithTypeConverter<TInterface, TType>( ) {
		_jsonConverters.Add( new KeyValuePair<Type, Type>( typeof( TInterface ), typeof( TType ) ) );

		return this;
	}

	/// <summary>
	/// Add a client factory to create a new HttpClient
	/// </summary>
	/// <param name="factory"></param>
	/// <param name="name"></param>
	/// <returns></returns>
	public ConfigurationBuilder WithHttpClientFactory( IHttpClientFactory factory, string name = "default" ) {
		_httpClient?.Dispose( );
		_httpClient = factory.CreateClient( name );
		return this;
	}

	/// <summary>
	/// Add a logger to log requests and responses
	/// </summary>
	/// <param name="logger"></param>
	/// <param name="logRequests"></param>
	/// <param name="logResponses"></param>
	/// <returns></returns>
	public ConfigurationBuilder WithLogging( ILogger logger, bool logRequests = true, bool logResponses = true ) {
		_logger = logger;
		_enableRequestLogging = logRequests;
		_enableResponseLogging = logResponses;

		return this;
	}

	/// <summary>
	/// Configure circuit breaker
	/// </summary>
	/// <param name="circuitBreaker">Circuit breaker instance</param>
	/// <returns>This object</returns>
	public ConfigurationBuilder WithCircuitBreaker( ICircuitBreaker circuitBreaker ) {
		_circuitBreaker = circuitBreaker ?? throw new ArgumentNullException( nameof( circuitBreaker ) );

		return this;
	}

	/// <summary>
	/// Configure circuit breaker
	/// </summary>
	/// <param name="options">Circuit breaker settings fluent</param>
	/// <returns>This object</returns>
	public ConfigurationBuilder WithCircuitBreaker( Action<CircuitBreakerSettings>? options ) {
		var settings = new CircuitBreakerSettings( );
		options?.Invoke( settings );

		var circuitBreaker = new CircuitBreaker(
			settings.FailureThreshold,
			settings.Timeout,
			settings.HalfOpenRetryTimeout );

		_circuitBreaker = circuitBreaker;

		return this;
	}

	/// <summary>
	/// Configure client certificate from PEM files
	/// </summary>
	/// <param name="certificatePath">Path to certificate file (.pem)</param>
	/// <param name="keyPath">Path to private key file (.key)</param>
	/// <param name="keyPassword">Optional password for private key</param>
	/// <returns>This object</returns>
	public ConfigurationBuilder WithCertificate( string certificatePath, string keyPath, string? keyPassword = null ) {
		ArgumentException.ThrowIfNullOrWhiteSpace( certificatePath, nameof( certificatePath ) );
		ArgumentException.ThrowIfNullOrWhiteSpace( keyPath, nameof( keyPath ) );

		_certificateOptions = new CertificateOptions {
			Type = CertificateType.PemWithKey,
			CertificatePath = certificatePath,
			KeyPath = keyPath,
			Password = keyPassword
		};

		return this;
	}

	/// <summary>
	/// Configure client certificate from PFX file
	/// </summary>
	/// <param name="pfxPath">Path to PFX file</param>
	/// <param name="password">Password for PFX file</param>
	/// <returns>This object</returns>
	public ConfigurationBuilder WithCertificate( string pfxPath, string password ) {
		ArgumentException.ThrowIfNullOrWhiteSpace( pfxPath, nameof( pfxPath ) );
		ArgumentException.ThrowIfNullOrWhiteSpace( password, nameof( password ) );

		_certificateOptions = new CertificateOptions {
			Type = CertificateType.Pfx,
			CertificatePath = pfxPath,
			Password = password
		};

		return this;
	}

	/// <summary>
	/// Configure client certificate from PFX byte array
	/// </summary>
	/// <param name="pfxData">PFX certificate data</param>
	/// <param name="password">Password for PFX data</param>
	/// <returns>This object</returns>
	public ConfigurationBuilder WithCertificate( byte[ ] pfxData, string password ) {
		ArgumentNullException.ThrowIfNull( pfxData, nameof( pfxData ) );
		ArgumentException.ThrowIfNullOrWhiteSpace( password, nameof( password ) );

		_certificateOptions = new CertificateOptions {
			Type = CertificateType.Pfx,
			CertificateData = pfxData,
			Password = password
		};

		return this;
	}

	/// <summary>
	/// Configure client certificate from an existing X509Certificate2 instance
	/// </summary>
	/// <param name="certificate">X509Certificate2 instance</param>
	/// <returns>This object</returns>
	public ConfigurationBuilder WithCertificate( X509Certificate2 certificate ) {
		ArgumentNullException.ThrowIfNull( certificate, nameof( certificate ) );

		_certificateOptions = new CertificateOptions {
			Type = CertificateType.X509Certificate2,
			Certificate = certificate
		};

		return this;
	}

	/// <summary>
	/// Configure client certificate from Windows certificate store by thumbprint
	/// </summary>
	/// <param name="thumbprint">Certificate thumbprint</param>
	/// <param name="storeLocation">Store location (default: CurrentUser)</param>
	/// <returns>This object</returns>
	public ConfigurationBuilder WithCertificateFromStore( string thumbprint, StoreLocation storeLocation = StoreLocation.CurrentUser ) {
		ArgumentException.ThrowIfNullOrWhiteSpace( thumbprint, nameof( thumbprint ) );

		_certificateOptions = new CertificateOptions {
			Type = CertificateType.Store,
			Thumbprint = thumbprint.Replace( " ", "" ).ToUpperInvariant( ),
			StoreLocation = storeLocation
		};

		return this;
	}

	/// <summary>
	/// Configure client certificate from Windows certificate store by subject name
	/// </summary>
	/// <param name="subjectName">Certificate subject name</param>
	/// <param name="storeLocation">Store location (default: CurrentUser)</param>
	/// <returns>This object</returns>
	public ConfigurationBuilder WithCertificateFromStoreBySubject( string subjectName, StoreLocation storeLocation = StoreLocation.CurrentUser ) {
		ArgumentException.ThrowIfNullOrWhiteSpace( subjectName, nameof( subjectName ) );

		_certificateOptions = new CertificateOptions {
			Type = CertificateType.Store,
			SubjectName = subjectName,
			StoreLocation = storeLocation
		};

		return this;
	}

	/// <summary>
	/// Configure client certificate with advanced options
	/// </summary>
	/// <param name="configure">Certificate configuration action</param>
	/// <returns>This object</returns>
	public ConfigurationBuilder WithCertificate( Action<CertificateOptions> configure ) {
		ArgumentNullException.ThrowIfNull( configure, nameof( configure ) );

		_certificateOptions = new CertificateOptions( );
		configure( _certificateOptions );

		return this;
	}
}
