using Myth.Extensions;
using Myth.Models.Rest;
using System.Net;

namespace Myth.Rest {

	public class ResultBuilder {
		private readonly ResultMappingList _resultMapping;

		public ResultBuilder( ) {
			_resultMapping = new ResultMappingList( );
		}

		public void Clear( ) {
			_resultMapping.Clear( );
		}

		public dynamic TryGet( HttpStatusCode statusCode, dynamic content, out Type? type ) {
			return _resultMapping.TryGet( statusCode, content, out type );
		}

		public ResultBuilder UseTypeForSuccess<TResult>( Func<dynamic, bool>? condition = null ) {
			return UseTypeForSuccess( typeof( TResult ), condition );
		}

		public ResultBuilder UseTypeForSuccess( Type type, Func<dynamic, bool>? condition = null ) {
			foreach ( var statusCode in Enum.GetValues<HttpStatusCode>( ) ) {
				if ( statusCode.IsSuccess( ) )
					UseTypeFor( statusCode, type, condition );
			}

			return this;
		}

		public ResultBuilder UseTypeFor<TResult>( HttpStatusCode statusCode, Func<dynamic, bool>? condition = null ) {
			return UseTypeFor( statusCode, typeof( TResult ), condition );
		}

		public ResultBuilder UseTypeFor( HttpStatusCode statusCode, Type type, Func<dynamic, bool>? condition = null ) {
			_resultMapping.Add( statusCode, type, condition );
			return this;
		}

		public ResultBuilder UseEmptyFor( HttpStatusCode statusCode, Func<dynamic, bool>? condition = null ) {
			_resultMapping.Add( statusCode, typeof( string ), condition );
			return this;
		}

		public ResultBuilder UseTypeFor( IEnumerable<HttpStatusCode> statusCodes, Type type, Func<dynamic, bool>? condition = null ) {
			foreach ( var statusCode in statusCodes ) {
				_resultMapping.Add( statusCode, type, condition );
			}

			return this;
		}

		public ResultBuilder UseTypeFor<TResult>( IEnumerable<HttpStatusCode> statusCodes, Func<dynamic, bool>? condition = null ) {
			return UseTypeFor( statusCodes, typeof( TResult ), condition );
		}

		public ResultBuilder UseTypeForAll<TResult>( Func<dynamic, bool>? condition = null ) {
			return UseTypeForAll( typeof( TResult ), condition );
		}

		public ResultBuilder UseTypeForAll( Type type, Func<dynamic, bool>? condition = null ) {
			foreach ( var statusCode in Enum.GetValues<HttpStatusCode>( ) ) {
				_resultMapping.Add( statusCode, type, condition );
			}

			return this;
		}
	}
}