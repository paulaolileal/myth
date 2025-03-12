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
	/// <param name="url">The url</param>
	/// <returns>This object</returns>
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

	/// <summary>
	/// Upload a file
	/// </summary>
	/// <param name="url">The url</param>
	/// <param name="body">The body</param>
	/// <param name="contentType">The content type</param>
	/// <param name="settings">Other settngs</param>
	/// <returns>This object</returns>
	public RestFileBuilder DoUpload<T>( string url, T body, string contentType, Action<RestUploadSettings>? settings = null ) {
		try {
			ArgumentNullException.ThrowIfNull( body, nameof( body ) );
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

	/// <summary>
	/// Upload a file
	/// </summary>
	/// <param name="url">The url</param>
	/// <param name="file">The file</param>
	/// <param name="settings">Other settings</param>
	/// <returns>This object</returns>
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

	/// <summary>
	/// Upload a file
	/// </summary>
	/// <param name="url">The url</param>
	/// <param name="file">The file</param>
	/// <param name="settings">Other settings</param>
	/// <returns>This object</returns>
	public RestFileBuilder DoUpload( string url, HttpContent content, Action<RestUploadSettings>? settings = null ) {
		try {
			ArgumentNullException.ThrowIfNull( content, nameof( content ) );
			ArgumentNullException.ThrowIfNull( content.Headers.ContentType, "content-type" );
			ArgumentNullException.ThrowIfNull( content.Headers.ContentLength, "content-length" );
			ArgumentOutOfRangeException.ThrowIfZero( content.Headers.ContentLength.Value, "content-length" );

			PreRequestSettings( );

			return DoUpload( url, content, content.Headers.ContentType.ToString( ), settings );
		} catch ( Exception exception ) {
			_exception = exception;
		}
		return this;
	}

	#endregion [ Actions ]

	#region [ Building ]

	/// <summary>
	/// Runs the request and get the response
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A task with the result</returns>

	public async Task<RestFileResponse> BuildAsync( CancellationToken cancellationToken = default ) =>
		await BuildAsync( null, cancellationToken );

	protected async Task<RestFileResponse> BuildAsync( Type? responseType = null, CancellationToken cancellationToken = default ) {
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