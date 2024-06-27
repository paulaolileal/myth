using Myth.Constants;
using System.Text;

namespace Myth.Extensions;

public static class HttpContentExtensions {

	public static HttpContent ToHttpContent( this object content, CaseStrategy caseStrategy = CaseStrategy.CamelCase ) =>
		new StringContent(
			content.ToJson( conf => conf.CaseStrategy = caseStrategy ),
			Encoding.UTF8,
			"application/json" );
}