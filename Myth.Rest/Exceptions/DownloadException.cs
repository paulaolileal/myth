namespace Myth.Exceptions {

    public class DownloadException : Exception {
        public string? FilePath { get; set; }
        public string? Url { get; set; }

        protected DownloadException( ) {
        }

        public DownloadException( string message, string filePath, string url )
            : base( $"Error on download file! {message}" ) {
            FilePath = filePath;
            Url = url;
        }
    }
}