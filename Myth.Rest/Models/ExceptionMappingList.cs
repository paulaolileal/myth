using System.Net;

namespace Myth.Models;

internal class ExceptionMappingList : Dictionary<HttpStatusCode, Func<dynamic, bool>?> {

	public new void Add( HttpStatusCode statusCode, Func<dynamic, bool>? condition ) {
		if ( ContainsKey( statusCode ) )
			Remove( statusCode );

		base.Add( statusCode, condition );
	}

	public bool TryGet( HttpStatusCode statusCode, dynamic content ) {
		var exists = TryGetValue( statusCode, out var value );

		return exists && value is null ||
			   exists && value is not null && value.Invoke( content );
	}
}