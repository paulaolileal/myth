using Myth.Constants;
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
}