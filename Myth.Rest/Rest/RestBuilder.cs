using Microsoft.AspNetCore.Http;
using Myth.Exceptions;
using Myth.Extensions;
using Myth.Models.Rest;
using Newtonsoft.Json;
using System.Dynamic;
using System.Net.Http.Headers;
using System.Text;

namespace Myth.Rest;

public partial class RestBuilder : RestBuilderBase {

	#region [ Pre-Request ]

	public override RestBuilder Configure( Action<ConfigurationBuilder>? configurationBuilder ) =>
		( base.Configure( configurationBuilder ) as RestBuilder )!;

	public RestBuilder OnResult( Action<ResultBuilder> resultSettings ) {
		_errorBuilder.Clear( );
		resultSettings.Invoke( _resultBuilder );
		return this;
	}

	public override RestBuilder OnError( Action<ErrorBuilder> exceptionSettings ) =>
		( base.OnError( exceptionSettings ) as RestBuilder )!;

	#endregion [ Pre-Request ]

	#region [ Building ]

	/// <summary>
	/// Runs the request and get the response
	/// </summary>
	/// <typeparam name="TResult">The result type</typeparam>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A task with the result</returns>
	public async Task<RestResponse> BuildAsync<TResult>( CancellationToken cancellationToken = default ) =>
		await BuildAsync( typeof( TResult ), cancellationToken );

	/// <summary>
	/// Runs the request and get the response
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A task with the result</returns>
	public async Task<RestResponse> BuildAsync( CancellationToken cancellationToken = default ) =>
		await BuildAsync( null, cancellationToken );

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

	#endregion [ Building ]

	#region [ Processing ]

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

	/// <summary>
	/// Applying retrying policy
	/// </summary>
	/// <param name="message"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	private async Task<(string stringContent, byte[ ] byteContent)> RetryingAsync( HttpResponseMessage message, CancellationToken cancellationToken ) {
		var retries = 0;
		string stringContent;
		byte[ ] byteContent;
		var timer = new PeriodicTimer( _configBuilder._retryPolicy.TimeBetweenRetry );

		do {
			byteContent = await message.Content.ReadAsByteArrayAsync( cancellationToken );
			stringContent = _isFileOperation ? "" : Encoding.UTF8.GetString( byteContent );
			retries++;
		} while (
			!message.StatusCode.IsSuccess( ) &&
			_configBuilder._retryPolicy.IsRetryStatusCode( message.StatusCode ) &&
			retries < _configBuilder._retryPolicy.AmountRetries &&
			await timer.WaitForNextTickAsync( cancellationToken ) );

		_configBuilder._retryPolicy.SetRetriesMade( retries );
		return (stringContent, byteContent);
	}

	#endregion [ Processing ]

	#region [ Content Actions ]

	/// <summary>
	/// Use a `GET` as method for request
	/// </summary>
	/// <param name="url">The url</param>
	/// <returns>This object</returns>
	public RestBuilder DoGet( string url ) {
		try {
			_isFileOperation = false;
			PreRequestSettings( );

			_request = async ( CancellationToken cancellationToken ) => await _configBuilder._httpClient.GetAsync( url, cancellationToken );
		} catch ( Exception exception ) {
			_exception = exception;
		}
		return this;
	}

	/// <summary>
	/// Use a `POST` as method for request
	/// </summary>
	/// <typeparam name="TBody">The type of body</typeparam>
	/// <param name="url">The url</param>
	/// <param name="body">The body</param>
	/// <returns>This object</returns>
	public RestBuilder DoPost<TBody>( string url, TBody? body = default ) {
		try {
			ArgumentNullException.ThrowIfNull( body, nameof( body ) );
			_isFileOperation = false;
			PreRequestSettings( );

			var request = body.ToHttpContent( _configBuilder._serializationCaseStrategy );

			_request = async ( CancellationToken cancellationToken ) => await _configBuilder._httpClient.PostAsync( url, request, cancellationToken );
		} catch ( Exception exception ) {
			_exception = exception;
		}
		return this;
	}

	/// <summary>
	/// Use a `PUT` as method for request
	/// </summary>
	/// <typeparam name="TBody">The type of body</typeparam>
	/// <param name="url">The url</param>
	/// <param name="body">The body</param>
	/// <returns>This object</returns>
	public RestBuilder DoPut<TBody>( string url, TBody? body = default ) {
		try {
			ArgumentNullException.ThrowIfNull( body, nameof( body ) );
			_isFileOperation = false;
			PreRequestSettings( );

			var request = body.ToHttpContent( _configBuilder._serializationCaseStrategy );

			_request = async ( CancellationToken cancellationToken ) => await _configBuilder._httpClient.PutAsync( url, request, cancellationToken );
		} catch ( Exception exception ) {
			_exception = exception;
		}
		return this;
	}

	/// <summary>
	/// Use a `DELETE` as method for request
	/// </summary>
	/// <param name="url">The url</param>
	/// <returns>This object</returns>
	public RestBuilder DoDelete( string url ) {
		try {
			_isFileOperation = false;
			PreRequestSettings( );

			_request = async ( CancellationToken cancellationToken ) => await _configBuilder._httpClient.DeleteAsync( url, cancellationToken );
		} catch ( Exception exception ) {
			_exception = exception;
		}
		return this;
	}

	/// <summary>
	/// Use a `PATCH` as method for request
	/// </summary>
	/// <typeparam name="TBody">The type of body</typeparam>
	/// <param name="url">The url</param>
	/// <param name="body">The body</param>
	/// <returns>This object</returns>
	public RestBuilder DoPatch<TBody>( string url, TBody? body = default ) {
		try {
			ArgumentNullException.ThrowIfNull( body, nameof( body ) );
			_isFileOperation = false;
			PreRequestSettings( );

			var request = body.ToHttpContent( _configBuilder._serializationCaseStrategy );

			_request = async ( CancellationToken cancellationToken ) => await _configBuilder._httpClient.PatchAsync( url, request, cancellationToken );
		} catch ( Exception exception ) {
			_exception = exception;
		}
		return this;
	}

	#endregion [ Content Actions ]

	#region [ File Actions ]

	/// <summary>
	/// Downloads a file
	/// </summary>
	/// <param name="url">The url</param>
	/// <returns>This object</returns>
	public RestBuilder DoDownload( string url ) {
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

	/// <summary>
	/// Upload a file
	/// </summary>
	/// <param name="url">The url</param>
	/// <param name="body">The body</param>
	/// <param name="contentType">The content type</param>
	/// <param name="settings">Other settings</param>
	/// <returns>This object</returns>
	public RestBuilder DoUpload<T>( string url, T body, string contentType, Action<RestUploadSettings>? settings = null ) {
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

	/// <summary>
	/// Upload a file
	/// </summary>
	/// <param name="url">The url</param>
	/// <param name="stream">The stream</param>
	/// <param name="contentType">The content type</param>
	/// <param name="settings">Other settings</param>
	/// <returns>This object</returns>
	public RestBuilder DoUpload( string url, Stream stream, string contentType, Action<RestUploadSettings>? settings = null ) {
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

	/// <summary>
	/// Upload a file
	/// </summary>
	/// <param name="url">The url</param>
	/// <param name="file">The file</param>
	/// <param name="settings">Other settings</param>
	/// <returns>This object</returns>
	public RestBuilder DoUpload( string url, IFormFile file, Action<RestUploadSettings>? settings = null ) {
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

	/// <summary>
	/// Upload a file
	/// </summary>
	/// <param name="url">The url</param>
	/// <param name="content">The HTTP content</param>
	/// <param name="settings">Other settings</param>
	/// <returns>This object</returns>
	public RestBuilder DoUpload( string url, HttpContent content, Action<RestUploadSettings>? settings = null ) {
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

	#endregion [ File Actions ]
}