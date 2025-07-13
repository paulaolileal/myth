using Myth.Exceptions;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace Myth.Rest;

public abstract class RestBuilderBase : IDisposable {
	protected readonly ErrorBuilder _errorBuilder;
	protected readonly ResultBuilder _resultBuilder;
	protected ConfigurationBuilder _configBuilder;

	protected Func<CancellationToken, Task<HttpResponseMessage>>? _request;
	protected Exception? _exception;
	protected Task<HttpResponseMessage>? _responseMessage;

	protected RestBuilderBase( ) {
		_errorBuilder = new( );
		_resultBuilder = new( );
		_configBuilder = new( );
	}

	/// <summary>
	/// Pre configure before the request
	/// </summary>
	/// <param name="configurationBuilder">Configurations</param>
	/// <returns>This object</returns>
	public virtual RestBuilderBase Configure( Action<ConfigurationBuilder>? configurationBuilder ) {
		configurationBuilder?.Invoke( _configBuilder );

		return this;
	}

	/// <summary>
	/// Defines the treatment for error cases
	/// </summary>
	/// <param name="exceptionSettings">Errors treatment</param>
	/// <returns>This object</returns>
	public virtual RestBuilderBase OnError( Action<ErrorBuilder> exceptionSettings ) {
		_errorBuilder.Clear( );
		exceptionSettings.Invoke( _errorBuilder );
		return this;
	}

	public void Dispose( ) {
		_configBuilder._httpClient?.Dispose( );

		_responseMessage?.Dispose( );

		GC.SuppressFinalize( this );
	}

	/// <summary>
	/// Make the request
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns>The response message and the time required</returns>
	/// <exception cref="NoActionMadeException">Throw it when no http method was made</exception>
	protected async Task<(HttpResponseMessage, TimeSpan)> ProcessAsync( CancellationToken cancellationToken = default ) {
		if ( _request is null )
			throw new NoActionMadeException( );

		var requestTime = new Stopwatch( );

		requestTime.Start( );

		var message = await _request.Invoke( cancellationToken );

		requestTime.Stop( );

		return (message, requestTime.Elapsed);
	}

	/// <summary>
	/// Set the parameters setted on configuration
	/// </summary>
	protected void PreRequestSettings( ) {
		var baseAddress = _configBuilder._baseUrl;
		if ( !string.IsNullOrEmpty( baseAddress ) )
			_configBuilder._httpClient.BaseAddress = new Uri( baseAddress );

		var authorization = _configBuilder._authorizationHeader;
		if ( authorization is not null )
			_configBuilder._httpClient.DefaultRequestHeaders.Authorization = authorization;

		var timeout = _configBuilder._timeout;
		if ( timeout is not null )
			_configBuilder._httpClient.Timeout = timeout.Value;

		var acceptableContentType = _configBuilder._acceptableContentType;
		if ( !string.IsNullOrEmpty( acceptableContentType ) )
			_configBuilder._httpClient
				.DefaultRequestHeaders
				.Accept
				.Add( new MediaTypeWithQualityHeaderValue( acceptableContentType ) );

		var customHeaders = _configBuilder._customHeaders;
		if ( customHeaders.Any( ) )
			foreach ( var header in customHeaders ) {
				if ( _configBuilder._httpClient.DefaultRequestHeaders.Contains( header.Key ) )
					_configBuilder._httpClient.DefaultRequestHeaders.Remove( header.Key );

				_configBuilder._httpClient.DefaultRequestHeaders.Add( header.Key, header.Value );
			}
	}

	/// <summary>
	/// Remove and clean resources for next request
	/// </summary>
	public void PostProcessing( ) {
		_request = null;
		_responseMessage = null;
		_exception = null;
		_configBuilder._httpClient.CancelPendingRequests( );
		_configBuilder._httpClient.DefaultRequestHeaders.Clear( );
		_configBuilder._httpClient.DefaultRequestHeaders.Accept.Clear( );
	}
}