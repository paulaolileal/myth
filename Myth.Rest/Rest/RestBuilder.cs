using Myth.Exceptions;
using Myth.Extensions;
using Myth.Models.Rest;
using Newtonsoft.Json;

namespace Myth.Rest;

public partial class RestBuilder : RestBuilderBase {

	#region [ Pre-Request ]

	public override RestBuilder Configure( Action<ConfigurationBuilder>? configurationBuilder ) =>
		( base.Configure( configurationBuilder ) as RestBuilder )!;

	public RestBuilder OnResult( Action<ResultBuilder> resultSettings ) {
		_exceptionBuilder.Clear( );
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

			return await ProcessRequestAsync( cancellationToken );
		} catch ( Exception exception ) {
			_exception = exception;
			throw _exception;
		}
	}

	#endregion [ Building ]

	#region [ Processing ]

	private async Task<RestResponse> ProcessRequestAsync( CancellationToken cancellationToken ) {
		var (message, elapsedTime) = await ProcessAsync( cancellationToken );

		var content = await RetringAsync( message, cancellationToken );

		var restResponse = new RestResponse(
			message.StatusCode,
			message.RequestMessage!.RequestUri!,
			message.RequestMessage.Method,
			content,
			elapsedTime,
			_configBuilder._retryPolicy.AmountRetriesMade );

		var dynamicContent = JsonConvert.DeserializeObject<dynamic>( content );

		if ( _exceptionBuilder.TryGet( message.StatusCode, dynamicContent ) )
			throw new NonSuccessException( restResponse );

		var mappedTypeExists = _resultBuilder.TryGet( message.StatusCode, dynamicContent, out Type? type );
		if ( _exceptionBuilder._throwForNonMappedResult ) {
			if ( !_resultBuilder.ShouldMap )
				return restResponse;
			else if ( !mappedTypeExists || type is null )
				throw new NotMappedResultTypeException( message.StatusCode, content );
			else if ( !string.IsNullOrEmpty( content ) ) {
				try {
					var typedResponse = content.FromJson( type!, conf => conf.UseCaseStrategy( _configBuilder._deserializationCaseStrategy ) );
					restResponse.SetTypedResult( type!, typedResponse! );
				} catch ( Exception exception ) {
					throw new ParsingTypeException( message.StatusCode, type!, content, exception );
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
	private async Task<string?> RetringAsync( HttpResponseMessage message, CancellationToken cancellationToken ) {
		var retries = 0;
		string? content;
		var timer = new PeriodicTimer( _configBuilder._retryPolicy.TimeBetweenRetry );

		do {
			content = await message.Content.ReadAsStringAsync( cancellationToken );
			retries++;
		} while (
			!message.StatusCode.IsSuccess( ) &&
			_configBuilder._retryPolicy.IsRetryStatusCode( message.StatusCode ) &&
			retries < _configBuilder._retryPolicy.AmountRetries &&
			await timer.WaitForNextTickAsync( cancellationToken ) );

		_configBuilder._retryPolicy.SetRetriesMade( retries );

		return content;
	}

	#endregion [ Processing ]

	#region [ Actions ]

	/// <summary>
	/// Use a `GET` as method for request
	/// </summary>
	/// <param name="url">The url</param>
	/// <returns>This object</returns>
	public RestBuilder DoGet( string url ) {
		try {
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
			PreRequestSettings( );

			var request = body.ToHttpContent( _configBuilder._serializationCaseStrategy );

			_request = async ( CancellationToken cancellationToken ) => await _configBuilder._httpClient.PatchAsync( url, request, cancellationToken );
		} catch ( Exception exception ) {
			_exception = exception;
		}
		return this;
	}

	#endregion [ Actions ]
}