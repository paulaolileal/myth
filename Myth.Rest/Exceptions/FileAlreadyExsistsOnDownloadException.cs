namespace Myth.Exceptions {

	public class FileAlreadyExsistsOnDownloadException : Exception {
		public string? FilePath { get; set; }
		public string? Url { get; set; }

		public FileAlreadyExsistsOnDownloadException( string message, string filePath, string url )
			: base( $"Error on download file! {message}" ) {
			FilePath = filePath;
			Url = url;
		}
	}
}