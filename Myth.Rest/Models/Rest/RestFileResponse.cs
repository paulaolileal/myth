using Myth.Exceptions;
using System.Net;

namespace Myth.Models.Rest {

	public class RestFileResponse : RestResponseBase {
		public byte[ ] Content { get; set; }

		public RestFileResponse(
			HttpStatusCode statusCode,
			Uri url,
			HttpMethod method,
			TimeSpan elapsedTime,
			byte[ ] content )
			: base( statusCode, url, method, elapsedTime ) {
			Content = content;
		}

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

		public Stream ToStream( ) {
			var stream = new MemoryStream( Content );

			return stream;
		}
	}
}