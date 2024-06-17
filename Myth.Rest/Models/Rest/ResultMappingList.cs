using System.Net;

namespace Myth.Models.Rest {

	internal class ResultMappingList : Dictionary<HttpStatusCode, (Type type, Func<dynamic, bool>? condition)> {

		public void Add( HttpStatusCode statusCode, Type type, Func<dynamic, bool>? condition ) {
			if ( ContainsKey( statusCode ) )
				Remove( statusCode );

			base.Add( statusCode, (type, condition) );
		}

		public bool TryGet( HttpStatusCode statusCode, dynamic content, out Type? type ) {
			var exists = TryGetValue( statusCode, out var value );

			type = value.type;

			return exists && value.condition is null ||
				   exists && value.condition is not null && value.condition.Invoke( content );
		}
	}
}