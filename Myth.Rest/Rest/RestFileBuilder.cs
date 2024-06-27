using Microsoft.AspNetCore.Http;
using Myth.Exceptions;
using Myth.Models.Rest;
using System.Dynamic;
using System.Net.Http.Headers;

namespace Myth.Rest;

public class RestFileBuilder : RestBuilderBase {

	#region [ Pre-Request ]

	public override RestFileBuilder Configure( Action<ConfigurationBuilder>? configurationBuilder ) =>
		( base.Configure( configurationBuilder ) as RestFileBuilder )!;

	public override RestFileBuilder OnError( Action<ErrorBuilder> exceptionSettings ) =>
		( base.OnError( exceptionSettings ) as RestFileBuilder )!;

	#endregion [ Pre-Request ]

	#region [ Actions ]

	/// <summary>
	/// Downloads a file
	/// </summary>
	/// <param name="url">Url of location</param>
	/// <param name="replaceExistingFile">If remove old file and create a new one</param>
	/// <returns></returns>
	/// <exception cref="FileAlreadyExsistsOnDownloadException"></exception>
	public RestFileBuilder DoDownload( string url ) {
		try {
			PreRequestSettings( );

			_request = async ( CancellationToken CancellationToken ) =>
				await _configBuilder._httpClient.GetAsync( url, cancellationToken: CancellationToken );
		} catch ( Exception exception ) {
			_exception = exception;
			throw _exception;
		}

		return this;
	}

	public RestFileBuilder DoUpload( string url, byte[ ] body, string contentType, Action<RestUploadSettings>? settings = null ) {
		try {
			ArgumentNullException.ThrowIfNull( body, nameof( body ) );
			PreRequestSettings( );

			var request = new ByteArrayContent( body ) {
				Headers = {
					ContentType = new MediaTypeHeaderValue(contentType)
				}
			};

			var uploadSettings = new RestUploadSettings( );
			if ( settings is not null )
				settings.Invoke( uploadSettings );

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

	public RestFileBuilder DoUpload( string url, Stream stream, string contentType, Action<RestUploadSettings>? settings = null ) {
		try {
			ArgumentNullException.ThrowIfNull( stream, nameof( stream ) );
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

	public RestFileBuilder DoUpload( string url, IFormFile file, Action<RestUploadSettings>? settings = null ) {
		try {
			ArgumentNullException.ThrowIfNull( file, nameof( file ) );
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

	#endregion [ Actions ]

	#region [ Building ]

	public async Task<RestFileResponse> BuildAsync<TResult>( CancellationToken cancellationToken = default ) =>
		await BuildAsync( typeof( TResult ), cancellationToken );

	public async Task<RestFileResponse> BuildAsync( CancellationToken cancellationToken = default ) =>
		await BuildAsync( null, cancellationToken );

	protected async Task<RestFileResponse> BuildAsync( Type? responseType = null, CancellationToken cancellationToken = default ) {
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

	private async Task<RestFileResponse> ProcessRequestAsync( CancellationToken cancellationToken ) {
		var (message, elapsedTime) = await ProcessAsync( cancellationToken );

		var content = await message.Content.ReadAsByteArrayAsync( cancellationToken );

		var restResponse = new RestFileResponse(
			message.StatusCode,
			message.RequestMessage!.RequestUri!,
			message.RequestMessage.Method,
			elapsedTime,
			content );

		if ( _exceptionBuilder.TryGet( message.StatusCode, new ExpandoObject( ) ) )
			throw new NonSuccessException( restResponse );

		return restResponse;
	}

	#endregion [ Processing ]
}