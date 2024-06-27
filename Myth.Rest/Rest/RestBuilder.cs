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
		resultSettings.Invoke( _statusBuilder );
		return this;
	}

	public override RestBuilder OnError( Action<ErrorBuilder> exceptionSettings ) =>
		( base.OnError( exceptionSettings ) as RestBuilder )!;

	#endregion [ Pre-Request ]

	#region [ Building ]

	public async Task<RestResponse> BuildAsync<TResult>( CancellationToken cancellationToken = default ) =>
		await BuildAsync( typeof( TResult ), cancellationToken );

	public async Task<RestResponse> BuildAsync( CancellationToken cancellationToken = default ) =>
		await BuildAsync( null, cancellationToken );

	protected async Task<RestResponse> BuildAsync( Type? responseType = null, CancellationToken cancellationToken = default ) {
		try {
			if ( responseType is not null ) {
				_statusBuilder.Clear( );
				_statusBuilder.UseTypeForAll( responseType );
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

		var content = await message.Content.ReadAsStringAsync( cancellationToken );

		var restResponse = new RestResponse(
			message.StatusCode,
			message.RequestMessage!.RequestUri!,
			message.RequestMessage.Method,
			content,
			elapsedTime );

		var dynamicContent = JsonConvert.DeserializeObject<dynamic>( content );

		if ( _exceptionBuilder.TryGet( message.StatusCode, dynamicContent ) )
			throw new NonSuccessException( restResponse );

		var mappedTypeExists = _statusBuilder.TryGet( message.StatusCode, dynamicContent, out Type? type );
		if ( _exceptionBuilder._throwForNonMappedResult ) {
			if ( !mappedTypeExists || type is null )
				throw new NotMappedResultTypeException( message.StatusCode );
			else if ( !string.IsNullOrEmpty( content ) ) {
				try {
					var typedResponse = content.FromJson( type!, conf => conf.CaseStrategy = _configBuilder._deserializationCaseStrategy );
					restResponse.SetTypedResult( type!, typedResponse! );
				} catch ( Exception exception ) {
					throw new ParsingTypeException( message.StatusCode, type!, content, exception );
				}
			}
		}

		return restResponse;
	}

	#endregion [ Processing ]

	#region [ Actions ]

	public RestBuilder DoGet( string url ) {
		try {
			PreRequestSettings( );

			_request = async ( CancellationToken cancellationToken ) => await _configBuilder._httpClient.GetAsync( url, cancellationToken );
		} catch ( Exception exception ) {
			_exception = exception;
		}
		return this;
	}

	public RestBuilder DoPost<TBody>( string url, TBody? body = default ) {
		try {
			ArgumentNullException.ThrowIfNull( body, nameof( body ) );
			PreRequestSettings( );

			var request = ToHttpContent( body ) ?? null;

			_request = async ( CancellationToken cancellationToken ) => await _configBuilder._httpClient.PostAsync( url, request, cancellationToken );
		} catch ( System.Exception exception ) {
			_exception = exception;
		}
		return this;
	}

	public RestBuilder DoPut<TBody>( string url, TBody? body = default ) {
		try {
			ArgumentNullException.ThrowIfNull( body, nameof( body ) );
			PreRequestSettings( );

			var request = ToHttpContent( body ) ?? null;

			_request = async ( CancellationToken cancellationToken ) => await _configBuilder._httpClient.PutAsync( url, request, cancellationToken );
		} catch ( System.Exception exception ) {
			_exception = exception;
		}
		return this;
	}

	public RestBuilder DoDelete( string url ) {
		try {
			PreRequestSettings( );

			_request = async ( CancellationToken cancellationToken ) => await _configBuilder._httpClient.DeleteAsync( url, cancellationToken );
		} catch ( System.Exception exception ) {
			_exception = exception;
		}
		return this;
	}

	public RestBuilder DoPatch<TBody>( string url, TBody? body = default ) {
		try {
			ArgumentNullException.ThrowIfNull( body, nameof( body ) );
			PreRequestSettings( );

			var request = ToHttpContent( body ) ?? null;

			_request = async ( CancellationToken cancellationToken ) => await _configBuilder._httpClient.PatchAsync( url, request, cancellationToken );
		} catch ( System.Exception exception ) {
			_exception = exception;
		}
		return this;
	}

	#endregion [ Actions ]

	#region [ Utils ]

	private HttpContent ToHttpContent<TBody>( TBody body ) {
		HttpContent request;
		if ( body is HttpContent content )
			request = content;
		else
			request = body!.ToHttpContent( _configBuilder._serializationCaseStrategy );
		return request;
	}

	#endregion [ Utils ]
}