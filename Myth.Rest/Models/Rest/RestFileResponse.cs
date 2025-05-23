using Myth.Exceptions;
using System.Net;
using System.Text;

namespace Myth.Models.Rest;

public class RestFileResponse(
	HttpStatusCode statusCode,
	Uri url,
	HttpMethod method,
	TimeSpan elapsedTime,
	int retriesMade,
	byte[ ] content )
	: RestResponseBase( statusCode, url, method, elapsedTime, retriesMade ) {
	public byte[ ] Content { get; set; } = content;

	/// <summary>
	/// Save the dowloaded stream into a local file
	/// </summary>
	/// <param name="directory">The directory</param>
	/// <param name="name">The file name</param>
	/// <param name="replaceExistingFile">Replace a existing file</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A task</returns>
	/// <exception cref="FileAlreadyExsistsOnDownloadException">Throws when a file with name already exists and replace options is not checked</exception>
	public async Task SaveToFileAsync( string directory, string name, bool replaceExistingFile = false, CancellationToken cancellationToken = default ) {
		var destinationPath = Path.Combine( directory, name );

		if ( File.Exists( destinationPath ) ) {
			if ( replaceExistingFile )
				File.Delete( destinationPath );
			else if ( !replaceExistingFile )
				throw new FileAlreadyExsistsOnDownloadException( "File already exists!", destinationPath, Url.ToString( ) );
		}

		await File.WriteAllBytesAsync( destinationPath, Content, cancellationToken );
	}

	/// <summary>
	/// Get a stream from current dowload
	/// </summary>
	/// <returns>A stream</returns>
	public Stream ToStream( ) => new MemoryStream( Content );

	/// <summary>
	/// Get the content as string
	/// </summary>
	/// <returns>Content encoded as string</returns>
	public override string ToString( ) => Encoding.Default.GetString( Content );
}