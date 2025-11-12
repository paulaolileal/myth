using System.Net;

namespace Myth.Exceptions;

/// <summary>
/// Exception thrown when the HTTP response content is not valid JSON
/// </summary>
public class InvalidJsonResponseException : Exception {

	/// <summary>
	/// The HTTP status code of the response
	/// </summary>
	public HttpStatusCode StatusCode { get; }

	/// <summary>
	/// The raw response content that could not be parsed as JSON
	/// </summary>
	public string RawContent { get; }

	/// <summary>
	/// The content type of the response
	/// </summary>
	public string? ContentType { get; }

	/// <summary>
	/// Creates a new instance of InvalidJsonResponseException
	/// </summary>
	/// <param name="statusCode">The HTTP status code</param>
	/// <param name="rawContent">The raw response content</param>
	/// <param name="contentType">The content type header</param>
	public InvalidJsonResponseException( HttpStatusCode statusCode, string rawContent, string? contentType = null )
		: base( CreateMessage( statusCode, rawContent, contentType ) ) {
		StatusCode = statusCode;
		RawContent = rawContent;
		ContentType = contentType;
	}

	/// <summary>
	/// Creates a new instance of InvalidJsonResponseException with an inner exception
	/// </summary>
	/// <param name="statusCode">The HTTP status code</param>
	/// <param name="rawContent">The raw response content</param>
	/// <param name="contentType">The content type header</param>
	/// <param name="innerException">The inner exception that caused this exception</param>
	public InvalidJsonResponseException( HttpStatusCode statusCode, string rawContent, string? contentType, Exception innerException )
		: base( CreateMessage( statusCode, rawContent, contentType ), innerException ) {
		StatusCode = statusCode;
		RawContent = rawContent;
		ContentType = contentType;
	}

	private static string CreateMessage( HttpStatusCode statusCode, string rawContent, string? contentType ) {
		var contentPreview = rawContent.Length > 200 ? rawContent[ ..200 ] + "..." : rawContent;
		var contentTypeInfo = !string.IsNullOrEmpty( contentType ) ? $" (Content-Type: {contentType})" : "";

		return $"Received non-JSON response with status {( int )statusCode} {statusCode}{contentTypeInfo}. " +
			   $"Content: {contentPreview}";
	}
}
