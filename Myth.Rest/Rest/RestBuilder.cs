using Microsoft.AspNetCore.Http;
using Myth.Exceptions;
using Myth.Extensions;
using Myth.Models.Rest;
using Myth.Rest.Interfaces;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Dynamic;
using System.Net.Http.Headers;
using System.Text;

namespace Myth.Rest;

/// <summary>
/// Unified REST builder that implements all fluent interfaces
/// </summary>
public class RestBuilder : IRestBuilder, IRestRequest, IRestPostProcessing,
	IRestErrorHandler, IRestResultHandler, IRestResult, IDisposable {

	#region [ Fields ]

	protected readonly ErrorBuilder _errorBuilder;
	protected readonly ResultBuilder _resultBuilder;
	protected ConfigurationBuilder _configBuilder;
	protected bool _isFileOperation;

	protected Func<CancellationToken, Task<HttpResponseMessage>>? _request;
	protected Exception? _exception;
	protected Task<HttpResponseMessage>? _responseMessage;

	#endregion [ Fields ]

	#region [ Constructor ]

	public RestBuilder( ) {
		_errorBuilder = new( );
		_resultBuilder = new( );
		_configBuilder = new( );
		_isFileOperation = false;
	}

	#endregion [ Constructor ]

	#region [ IRestConfiguration Implementation ]

	public IRestRequest Configure( Action<ConfigurationBuilder> configurationBuilder ) {
		configurationBuilder?.Invoke( _configBuilder );
		return this;
	}

	#endregion [ IRestConfiguration Implementation ]

	#region [ IRestActions Implementation ]

	public IRestPostProcessing DoGet( string url ) {
		try {
			_isFileOperation = false;
			PreRequestSettings( );

			_request = async ( CancellationToken cancellationToken ) =>
				await _configBuilder._httpClient.GetAsync( url, cancellationToken );
		} catch ( Exception exception ) {
			_exception = exception;
		}
		return this;
	}

	public IRestPostProcessing DoPost<TBody>( string url, TBody? body = default ) {
		try {
			ArgumentNullException.ThrowIfNull( body, nameof( body ) );
			_isFileOperation = false;
			PreRequestSettings( );

			var request = body.ToHttpContent( _configBuilder._serializationCaseStrategy );

			_request = async ( CancellationToken cancellationToken ) =>
				await _configBuilder._httpClient.PostAsync( url, request, cancellationToken );
		} catch ( Exception exception ) {
			_exception = exception;
		}
		return this;
	}

	public IRestPostProcessing DoPut<TBody>( string url, TBody? body = default ) {
		try {
			ArgumentNullException.ThrowIfNull( body, nameof( body ) );
			_isFileOperation = false;
			PreRequestSettings( );

			var request = body.ToHttpContent( _configBuilder._serializationCaseStrategy );

			_request = async ( CancellationToken cancellationToken ) =>
				await _configBuilder._httpClient.PutAsync( url, request, cancellationToken );
		} catch ( Exception exception ) {
			_exception = exception;
		}
		return this;
	}

	public IRestPostProcessing DoDelete( string url ) {
		try {
			_isFileOperation = false;
			PreRequestSettings( );

			_request = async ( CancellationToken cancellationToken ) =>
				await _configBuilder._httpClient.DeleteAsync( url, cancellationToken );
		} catch ( Exception exception ) {
			_exception = exception;
		}
		return this;
	}

	public IRestPostProcessing DoPatch<TBody>( string url, TBody? body = default ) {
		try {
			ArgumentNullException.ThrowIfNull( body, nameof( body ) );
			_isFileOperation = false;
			PreRequestSettings( );

			var request = body.ToHttpContent( _configBuilder._serializationCaseStrategy );

			_request = async ( CancellationToken cancellationToken ) =>
				await _configBuilder._httpClient.PatchAsync( url, request, cancellationToken );
		} catch ( Exception exception ) {
			_exception = exception;
		}
		return this;
	}

	public IRestPostProcessing DoDownload( string url ) {
		try {
			_isFileOperation = true;
			PreRequestSettings( );

			_request = async ( CancellationToken cancellationToken ) =>
				await _configBuilder._httpClient.GetAsync( url, cancellationToken: cancellationToken );
		} catch ( Exception exception ) {
			_exception = exception;
		}

		return this;
	}

	public IRestPostProcessing DoUpload<T>( string url, T body, string contentType, Action<RestUploadSettings>? settings = null ) {
		try {
			ArgumentNullException.ThrowIfNull( body, nameof( body ) );
			_isFileOperation = true;
			PreRequestSettings( );

			HttpContent? request = null;
			if ( body is HttpContent httpContent ) {
				request = httpContent;
			} else if ( body is byte[ ] content ) {
				request = new ByteArrayContent( content ) {
					Headers = {
						ContentType = new MediaTypeHeaderValue(contentType)
					}
				};
			}

			var uploadSettings = new RestUploadSettings( );
			settings?.Invoke( uploadSettings );

			_request = async ( CancellationToken cancellationToken ) => {
				return uploadSettings.Method switch {
					RestUploadSettings.UploadMethod.PUT => await _configBuilder._httpClient.PutAsync( url, request, cancellationToken ),
					RestUploadSettings.UploadMethod.PATCH => await _configBuilder._httpClient.PatchAsync( url, request, cancellationToken ),
					_ => await _configBuilder._httpClient.PostAsync( url, request, cancellationToken )
				};
			};
		} catch ( Exception exception ) {
			_exception = exception;
		}
		return this;
	}

	public IRestPostProcessing DoUpload( string url, Stream stream, string contentType, Action<RestUploadSettings>? settings = null ) {
		try {
			ArgumentNullException.ThrowIfNull( stream, nameof( stream ) );
			_isFileOperation = true;
			PreRequestSettings( );

			using var memoryStream = new MemoryStream( );
			stream.CopyTo( memoryStream );
			memoryStream.Position = 0;
			var body = memoryStream.ToArray( );

			return DoUpload( url, body, contentType, settings );
		} catch ( Exception exception ) {
			_exception = exception;
		}
		return this;
	}

	public IRestPostProcessing DoUpload( string url, IFormFile file, Action<RestUploadSettings>? settings = null ) {
		try {
			ArgumentNullException.ThrowIfNull( file, nameof( file ) );
			_isFileOperation = true;
			PreRequestSettings( );

			using var memoryStream = new MemoryStream( );
			file.CopyTo( memoryStream );
			memoryStream.Position = 0;
			var body = memoryStream.ToArray( );

			return DoUpload( url, body, file.ContentType, settings );
		} catch ( Exception exception ) {
			_exception = exception;
		}
		return this;
	}

	public IRestPostProcessing DoUpload( string url, HttpContent content, Action<RestUploadSettings>? settings = null ) {
		try {
			ArgumentNullException.ThrowIfNull( content, nameof( content ) );
			ArgumentNullException.ThrowIfNull( content.Headers.ContentType, "content-type" );
			ArgumentNullException.ThrowIfNull( content.Headers.ContentLength, "content-length" );
			ArgumentOutOfRangeException.ThrowIfZero( content.Headers.ContentLength.Value, "content-length" );

			_isFileOperation = true;
			PreRequestSettings( );

			return DoUpload( url, content, content.Headers.ContentType.ToString( ), settings );
		} catch ( Exception exception ) {
			_exception = exception;
		}
		return this;
	}

	#endregion [ IRestActions Implementation ]

	#region [ IRestPostProcessing Implementation ]

	public IRestErrorHandler OnError( Action<ErrorBuilder> errorSettings ) {
		_errorBuilder.Clear( );
		errorSettings.Invoke( _errorBuilder );
		return this;
	}

	public IRestResultHandler OnResult( Action<ResultBuilder> resultSettings ) {
		_resultBuilder.Clear( );
		resultSettings.Invoke( _resultBuilder );
		return this;
	}

	#endregion [ IRestPostProcessing Implementation ]

	#region [ IRestPostProcessingError Implementation ]

	IRestResult IRestErrorHandler.OnResult( Action<ResultBuilder> resultSettings ) {
		_resultBuilder.Clear( );
		resultSettings.Invoke( _resultBuilder );
		return this;
	}

	#endregion [ IRestPostProcessingError Implementation ]

	#region [ IRestPostProcessingResult Implementation ]

	IRestResult IRestResultHandler.OnError( Action<ErrorBuilder> errorSettings ) {
		_errorBuilder.Clear( );
		errorSettings.Invoke( _errorBuilder );
		return this;
	}

	#endregion [ IRestPostProcessingResult Implementation ]

	#region [ Build Methods ]

	public async Task<RestResponse> BuildAsync( CancellationToken cancellationToken = default ) =>
		await BuildAsync( null, cancellationToken );

	public async Task<RestResponse> BuildAsync<TResult>( CancellationToken cancellationToken = default ) =>
		await BuildAsync( typeof( TResult ), cancellationToken );

	protected async Task<RestResponse> BuildAsync( Type? responseType = null, CancellationToken cancellationToken = default ) {
		try {
			if ( responseType is not null ) {
				_resultBuilder.Clear( );
				_resultBuilder.UseTypeForAll( responseType );
			}

			var response = await ProcessRequestAsync( cancellationToken );

			PostProcessing( );

			return response;
		} catch ( Exception exception ) {
			_exception = exception;
			throw _exception;
		}
	}

	#endregion [ Build Methods ]

	#region [ Processing Methods ]

	private async Task<RestResponse> ProcessRequestAsync( CancellationToken cancellationToken ) {
		var (message, elapsedTime) = await ProcessAsync( cancellationToken );

		// Retry logic and content processing
		var (stringContent, byteContent) = await RetryingAsync( message, cancellationToken );

		// Fallback logic
		if ( !message.StatusCode.IsSuccess( ) && _errorBuilder._useFallback ) {
			message.StatusCode = _errorBuilder._fallbackStatusCode!.Value;
			stringContent = _errorBuilder._fallbackResponse ?? "";
			byteContent = Encoding.UTF8.GetBytes( stringContent );
		}

		var restResponse = new RestResponse(
			message.StatusCode,
			message.RequestMessage!.RequestUri!,
			message.RequestMessage.Method,
			stringContent,
			byteContent,
			elapsedTime,
			_configBuilder._retryPolicy.AmountRetriesMade,
			_errorBuilder._useFallback,
			_isFileOperation );

		// Handle error checking - for file operations, use empty dynamic object
		dynamic dynamicContent = _isFileOperation ? new ExpandoObject( ) : JsonConvert.DeserializeObject<dynamic>( stringContent );

		// If the response is not success and there is no fallback, throw an exception
		if ( _errorBuilder.TryGet( message.StatusCode, dynamicContent ) )
			throw new NonSuccessException( restResponse );

		// Only map types for non-file operations
		if ( !_isFileOperation ) {
			var mappedTypeExists = _resultBuilder.TryGet( message.StatusCode, dynamicContent, out Type? type );

			if ( _resultBuilder.ShouldMap ) {
				if ( _errorBuilder._throwForNonMappedResult && ( !mappedTypeExists || type is null ) )
					throw new NotMappedResultTypeException( message.StatusCode, stringContent );
				else if ( !string.IsNullOrEmpty( stringContent ) && type is not null ) {
					try {
						var typedResponse = stringContent.FromJson( type, conf => {
							conf.UseCaseStrategy( _configBuilder._deserializationCaseStrategy );
							foreach ( var (interfaceType, concreteType) in _configBuilder._jsonConverters )
								conf.UseInterfaceConverter( interfaceType, concreteType );
						} );
						restResponse.SetTypedResult( type, typedResponse! );
					} catch ( Exception exception ) {
						throw new ParsingTypeException( message.StatusCode, type, stringContent, exception );
					}
				}
			}
		}

		return restResponse;
	}

	private async Task<(string stringContent, byte[ ] byteContent)> RetryingAsync( HttpResponseMessage message, CancellationToken cancellationToken ) {
		var retries = 0;
		string stringContent;
		byte[ ] byteContent;

		var shouldRetry = true;

		while ( shouldRetry && retries <= _configBuilder._retryPolicy.AmountRetries ) {
			try {
				byteContent = await message.Content.ReadAsByteArrayAsync( cancellationToken );
				stringContent = _isFileOperation ? "" : Encoding.UTF8.GetString( byteContent );

				// If success, get off the loop
				if ( message.StatusCode.IsSuccess( ) ) 
					break;				

				// Checks if must retry
				shouldRetry = retries < _configBuilder._retryPolicy.AmountRetries &&
							 _configBuilder._retryPolicy.IsRetryStatusCode( message.StatusCode );

				if ( shouldRetry ) {
					// Calculate delay based on strategy
					var delay = _configBuilder._retryPolicy.CalculateDelay( retries + 1 );
					await Task.Delay( delay, cancellationToken );

					// Retry the request
					message.Dispose( ); // Clean the previous answer
					message = await _request!.Invoke( cancellationToken );
					retries++;
				}
			} catch ( Exception ex ) 
				when ( _configBuilder._retryPolicy.IsRetryException( ex ) && 
					  retries < _configBuilder._retryPolicy.AmountRetries ) {

				// Retry if is a setted exception
				var delay = _configBuilder._retryPolicy.CalculateDelay( retries + 1 );

				await Task.Delay( delay, cancellationToken );

				try {
					message = await _request!.Invoke( cancellationToken );
					retries++;
				} catch {
					// If failed throws the exception
					throw;
				}
			}
		}

		// Get the last answer
		byteContent = await message.Content.ReadAsByteArrayAsync( cancellationToken );

		stringContent = _isFileOperation ? "" : Encoding.UTF8.GetString( byteContent );

		_configBuilder._retryPolicy.SetRetriesMade( retries );

		return (stringContent, byteContent);
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

	public void PostProcessing( ) {
		_request = null;
		_responseMessage = null;
		_exception = null;
		_isFileOperation = false;
		_configBuilder._httpClient.CancelPendingRequests( );
		_configBuilder._httpClient.DefaultRequestHeaders.Clear( );
		_configBuilder._httpClient.DefaultRequestHeaders.Accept.Clear( );
	}

	#endregion [ Processing Methods ]

	#region [ IDisposable Implementation ]

	public void Dispose( ) {
		_configBuilder._httpClient?.Dispose( );
		_responseMessage?.Dispose( );
		GC.SuppressFinalize( this );
	}

	#endregion [ IDisposable Implementation ]
}