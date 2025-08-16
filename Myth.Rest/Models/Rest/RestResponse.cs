using Myth.Exceptions;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace Myth.Models.Rest;

public class RestResponse(
		HttpStatusCode statusCode,
		Uri url,
		HttpMethod method,
		string rawMessage,
		byte[ ] rawBytes,
		TimeSpan elapsedTime,
		int retriesMade,
		bool fallbackUsed,
		bool isFileResponse = false )
	: RestResponseBase( statusCode, url, method, elapsedTime, retriesMade, fallbackUsed ) {
	public string RawMessage { get; private set; } = rawMessage;
	public byte[ ] RawBytes { get; private set; } = rawBytes;
	public bool IsFileResponse { get; private set; } = isFileResponse;
	public Type? ResultType { get; private set; }
	public object? Result { get; private set; }

	public dynamic DynamicResult { get; private set; } =
		isFileResponse ? new { } : JsonConvert.DeserializeObject<dynamic>( rawMessage )!;

	internal void SetTypedResult( Type type, object result ) {
		ResultType = type;
		Result = result;
	}

	/// <summary>
	/// Get the result typed with the mapped type
	/// </summary>
	/// <typeparam name="TResult">The mapped type</typeparam>
	/// <returns>The strongly typed object</returns>
	/// <exception cref="DifferentResponseTypeException">Throws when the <c>TResult</c> is different from mapped type</exception>
	public TResult GetAs<TResult>( ) {
		if ( Result is not null && ResultType == typeof( TResult ) )
			return ( TResult )Result;

		throw new DifferentResponseTypeException( typeof( TResult ), ResultType );
	}

	/// <summary>
	/// Save the response content (file or text) to a local file
	/// </summary>
	/// <param name="directory">The directory</param>
	/// <param name="name">The file name</param>
	/// <param name="replaceExistingFile">Replace an existing file</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A task</returns>
	/// <exception cref="FileAlreadyExsistsOnDownloadException">Throws when a file with name already exists and replace option is not checked</exception>
	public async Task SaveToFileAsync( string directory, string name, bool replaceExistingFile = false, CancellationToken cancellationToken = default ) {
		var destinationPath = Path.Combine( directory, name );

		if ( File.Exists( destinationPath ) ) {
			if ( replaceExistingFile )
				File.Delete( destinationPath );
			else
				throw new FileAlreadyExsistsOnDownloadException( "File already exists!", destinationPath, Url.ToString( ) );
		}

		// Use raw bytes if it's a file response, otherwise convert text to bytes
		var bytesToWrite = IsFileResponse ? RawBytes : Encoding.UTF8.GetBytes( RawMessage );
		await File.WriteAllBytesAsync( destinationPath, bytesToWrite, cancellationToken );
	}

	/// <summary>
	/// Get a stream from current response content
	/// </summary>
	/// <returns>A stream</returns>
	public Stream ToStream( ) => new MemoryStream( IsFileResponse ? RawBytes : Encoding.UTF8.GetBytes( RawMessage ) );

	/// <summary>
	/// Get the content as string
	/// </summary>
	/// <returns>Content encoded as string</returns>
	public override string ToString( ) => IsFileResponse ? Encoding.UTF8.GetString( RawBytes ) : RawMessage;

	/// <summary>
	/// Get the content as byte array
	/// </summary>
	/// <returns>Content as byte array</returns>
	public byte[ ] ToByteArray( ) => IsFileResponse ? RawBytes : Encoding.UTF8.GetBytes( RawMessage );
}