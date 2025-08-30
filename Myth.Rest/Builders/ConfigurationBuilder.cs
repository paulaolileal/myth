using Microsoft.Extensions.Logging;
using Myth.Constants;
using Myth.Interfaces;
using Myth.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

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

	private bool _ownsHttpClient = true;

	private static readonly HashSet<string> SensitiveHeaders = new(StringComparer.OrdinalIgnoreCase) {
		"Authorization", "X-API-Key", "Cookie", "X-Auth-Token"
	};

	public ConfigurationBuilder() {
		_httpClient = new HttpClient();
		_customHeaders = new Dictionary<string, string>();
		_retryPolicy = new RetryPolicy();
		_serializationCaseStrategy = CaseStrategy.CamelCase;
		_deserializationCaseStrategy = CaseStrategy.CamelCase;
		_jsonConverters = new Dictionary<Type, Type>();
	}

	public void Dispose() {
		if (_ownsHttpClient) _httpClient?.Dispose();
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Set a pre-built http client
	/// </summary>
	/// <param name="httpClient">Http client</param>
	/// <returns>This object</returns>
	public ConfigurationBuilder WithClient(HttpClient httpClient) {
		_httpClient?.Dispose();
		_httpClient = httpClient;
		_ownsHttpClient = false; // Não deve fazer dispose de cliente externo

		return this;
	}

	/// <summary>
	/// Set a base url for request
	/// </summary>
	/// <param name="baseUrl">A base url</param>
	/// <returns>This object</returns>
	public ConfigurationBuilder WithBaseUrl(string baseUrl) {
		_baseUrl = baseUrl;
		return this;
	}

	/// <summary>
	/// Set a timeout until aborts the request
	/// </summary>
	/// <param name="timeout">The timeout</param>
	/// <returns>This object</returns>
	public ConfigurationBuilder WithTimeout(TimeSpan timeout) {
		_timeout = timeout;
		return this;
	}

	/// <summary>
	/// Set a authorization header
	/// </summary>
	/// <param name="scheme">Authorization schema</param>
	/// <param name="token">The token</param>
	/// <returns>This object</returns>
	public ConfigurationBuilder WithAuthorization(string scheme, string token) {
		_authorizationHeader = new AuthenticationHeaderValue(scheme, token);
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
	public ConfigurationBuilder WithBearerAuthorization(string token) {
		WithAuthorization("Bearer", token);
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
	public ConfigurationBuilder WithBasicAuthorization(string username, string password) {
		var encodedtoken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));

		return WithBasicAuthorization(encodedtoken);
	}

	/// <summary>
	/// Set a basic authorization header
	/// </summary>
	/// <param name="encodedToken">The token</param>
	/// <returns>This object</returns>
	/// <remarks>
	/// The word `Basic` doesn't need to be ironed
	/// </remarks>
	public ConfigurationBuilder WithBasicAuthorization(string encodedToken) {
		WithAuthorization("Basic", encodedToken);

		return this;
	}

	/// <summary>
	/// Set a acceptable content type
	/// </summary>
	/// <param name="contentType">The content type</param>
	/// <returns>This object</returns>
	public ConfigurationBuilder WithContentType(string contentType) {
		_acceptableContentType = contentType;
		return this;
	}

	/// <summary>
	/// Add a header
	/// </summary>
	/// <param name="key">Header key</param>
	/// <param name="value">Header value</param>
	/// <returns>This object</returns>
	public ConfigurationBuilder WithHeader(string key, string value) {
		ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));
		ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));

		if (_customHeaders.ContainsKey(key))
			_customHeaders.Remove(key);

		if (SensitiveHeaders.Contains(key))
			_logger?.LogWarning("Adding sensitive header {HeaderName}. Consider using specific authorization methods.", key);

		_customHeaders.TryAdd(key, value);

		return this;
	}

	/// <summary>
	/// Set the serialization of request body
	/// </summary>
	/// <param name="serializationCaseStrategy">The case of strategy</param>
	/// <returns>This object</returns>
	public ConfigurationBuilder WithBodySerialization(CaseStrategy serializationCaseStrategy) {
		_serializationCaseStrategy = serializationCaseStrategy;
		return this;
	}

	/// <summary>
	/// Set the serialization of response body
	/// </summary>
	/// <param name="deserializationCaseStrategy">The case of strategy</param>
	/// <returns>This object</returns>
	public ConfigurationBuilder WithBodyDeserialization(CaseStrategy deserializationCaseStrategy) {
		_deserializationCaseStrategy = deserializationCaseStrategy;
		return this;
	}

	/// <summary>
	/// Set default retry (3 tries, backoff exponential with jitter)
	/// </summary>
	public ConfigurationBuilder WithRetry() {
		_retryPolicy = new RetryPolicy()
			.WithMaxAttempts(3)
			.UseExponentialBackoffWithJitter(TimeSpan.FromSeconds(1))
			.ForServerErrors();

		return this;
	}

	/// <summary>
	/// Set custom retry police
	/// </summary>
	public ConfigurationBuilder WithRetry(Action<RetryPolicy> configure) {
		configure(_retryPolicy);
		return this;
	}

	/// <summary>
	/// Set basic retry
	/// </summary>
	public ConfigurationBuilder WithRetry(int amount, TimeSpan timeBetweenRetries, params HttpStatusCode[] statusCodes) {
		_retryPolicy.Set(amount, timeBetweenRetries, statusCodes);
		return this;
	}

	/// <summary>
	/// Add a custom converter for type on deserialization of response
	/// </summary>
	/// <typeparam name="TInterface"></typeparam>
	/// <typeparam name="TType"></typeparam>
	/// <returns></returns>
	public ConfigurationBuilder WithTypeConverter<TInterface, TType>() {
		_jsonConverters.Add(new KeyValuePair<Type, Type>(typeof(TInterface), typeof(TType)));

		return this;
	}

	/// <summary>
	/// Add a client factory to create a new HttpClient
	/// </summary>
	/// <param name="factory"></param>
	/// <param name="name"></param>
	/// <returns></returns>
	public ConfigurationBuilder WithHttpClientFactory(IHttpClientFactory factory, string name = "default") {
		_httpClient?.Dispose();
		_httpClient = factory.CreateClient(name);
		return this;
	}

	/// <summary>
	/// Add a logger to log requests and responses
	/// </summary>
	/// <param name="logger"></param>
	/// <param name="logRequests"></param>
	/// <param name="logResponses"></param>
	/// <returns></returns>
	public ConfigurationBuilder WithLogging(ILogger logger, bool logRequests = true, bool logResponses = true) {
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
	public ConfigurationBuilder WithCircuitBreaker(ICircuitBreaker circuitBreaker) {
		_circuitBreaker = circuitBreaker ?? throw new ArgumentNullException(nameof(circuitBreaker));

		return this;
	}

	/// <summary>
	/// Configure circuit breaker
	/// </summary>
	/// <param name="options">Circuit breaker settings fluent</param>
	/// <returns>This object</returns>
	public ConfigurationBuilder WithCircuitBreaker(Action<CircuitBreakerSettings>? options) {
		var settings = new CircuitBreakerSettings();
		options?.Invoke(settings);

		var circuitBreaker = new CircuitBreaker(
			settings.FailureThreshold,
			settings.Timeout,
			settings.HalfOpenRetryTimeout);

		_circuitBreaker = circuitBreaker;

		return this;
	}
}