namespace Myth.Exceptions;

public class FileAlreadyExsistsOnDownloadException( string message, string filePath, string url ) : Exception( $"Error on download file! {message}" ) {
	public string? FilePath { get; set; } = filePath;
	public string? Url { get; set; } = url;
}