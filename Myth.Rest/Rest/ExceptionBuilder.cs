using Myth.Extensions;
using Myth.Models.Rest;
using System.Net;

namespace Myth.Rest {

	public class ExceptionBuilder {
		private readonly ExceptionMappingList _exceptionMapping;
		public bool _throwForNonMappedResult;

		public ExceptionBuilder( ) {
			_exceptionMapping = new( );
			_throwForNonMappedResult = true;
		}

		public void Clear( ) {
			_exceptionMapping.Clear( );
		}

		public dynamic TryGet( HttpStatusCode statusCode, dynamic content ) {
			return _exceptionMapping.TryGet( statusCode, content );
		}

		public ExceptionBuilder ThrowForNonSuccess( Func<dynamic, bool>? condition = null ) {
			foreach ( var statusCode in Enum.GetValues<HttpStatusCode>( ) ) {
				if ( !statusCode.IsSuccess( ) )
					ThrowFor( statusCode, condition );
			}

			return this;
		}

		public ExceptionBuilder ThrowFor( HttpStatusCode statusCode, Func<dynamic, bool>? condition = null ) {
			_exceptionMapping.Add( statusCode, condition );
			return this;
		}

		public ExceptionBuilder ThrowFor( List<HttpStatusCode> statusCodes, Func<dynamic, bool>? condition = null ) {
			foreach ( var statusCode in statusCodes ) {
				ThrowFor( statusCode, condition );
			}

			return this;
		}

		public ExceptionBuilder ThrowForAll( Func<dynamic, bool>? condition = null ) {
			foreach ( var statusCode in Enum.GetValues<HttpStatusCode>( ) ) {
				_exceptionMapping.Add( statusCode, condition );
			}

			return this;
		}

		public ExceptionBuilder NotThrowForNonMappedResult( ) {
			_throwForNonMappedResult = false;

			return this;
		}

		public ExceptionBuilder NotThrowFor( HttpStatusCode statusCode, Func<dynamic, bool>? condition = null ) {
			_exceptionMapping.Remove( statusCode );

			return this;
		}
	}
}