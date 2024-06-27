using Myth.Exceptions;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace Myth.Rest;

public abstract class RestBuilderBase : IDisposable {
	protected readonly ErrorBuilder _exceptionBuilder;
	protected readonly ResultBuilder _statusBuilder;
	protected Exception? _exception;
	protected Func<CancellationToken, Task<HttpResponseMessage>>? _request;

	protected ConfigurationBuilder _configBuilder;
	protected Task<HttpResponseMessage>? _responseMessage;

	protected RestBuilderBase( ) {
		_exceptionBuilder = new( );
		_statusBuilder = new( );
		_configBuilder = new( );
	}

	public virtual RestBuilderBase Configure( Action<ConfigurationBuilder>? configurationBuilder ) {
		configurationBuilder?.Invoke( _configBuilder );

		return this;
	}

	public virtual RestBuilderBase OnError( Action<ErrorBuilder> exceptionSettings ) {
		_exceptionBuilder.Clear( );
		exceptionSettings.Invoke( _exceptionBuilder );
		return this;
	}

	public void Dispose( ) {
		_configBuilder._httpClient?.Dispose( );

		_responseMessage?.Dispose( );
	}

	protected async Task<(HttpResponseMessage, TimeSpan)> ProcessAsync( CancellationToken cancellationToken = default ) {
		if ( _request is null )
			throw new NoActionMadeException( );

		var requestTime = new Stopwatch( );

		requestTime.Start( );

		var message = await _request.Invoke( cancellationToken );

		requestTime.Stop( );

		return (message, requestTime.Elapsed);
	}

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
}