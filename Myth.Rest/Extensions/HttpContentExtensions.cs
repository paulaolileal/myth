using Microsoft.AspNetCore.Http;
using Myth.Constants;
using System.Net.Http.Headers;
using System.Text;

namespace Myth.Extensions;

public static class HttpContentExtensions {

	/// <summary>
	/// Turns any element in http content
	/// </summary>
	/// <param name="content">The content</param>
	/// <param name="caseStrategy">The case strategy to apply</param>
	/// <returns></returns>
	public static HttpContent ToHttpContent( this object content, CaseStrategy caseStrategy = CaseStrategy.CamelCase ) =>
		new StringContent(
			content.ToJson( conf => conf.UseCaseStrategy( caseStrategy ) ),
			Encoding.UTF8,
			"application/json" );

	/// <summary>
	/// Turns a file into a multi part form data
	/// </summary>
	/// <param name="file">The form file object</param>
	/// <returns>A multipart form data content</returns>
	public static MultipartFormDataContent ToMultiPartFormData( this IFormFile file ) {
		var multiPartContent = new MultipartFormDataContent( );
		var streamContent = new StreamContent( file.OpenReadStream( ) );
		streamContent.Headers.ContentType = new MediaTypeHeaderValue( file.ContentType );
		multiPartContent.Add( streamContent, "file", file.FileName );

		return multiPartContent;
	}
}