using Myth.Models.Rest;
using System.Net;

namespace Myth.Rest {

    public class RestStatusBuilder {
        private readonly MappingResultList _mapStatus;
        private readonly IDictionary<HttpStatusCode, bool> _mapException;
        private Type? _allMapStatus;
        private bool _throwOnNonSuccess;

        public RestStatusBuilder( ) {
            _mapStatus = new MappingResultList( );
            _mapException = new Dictionary<HttpStatusCode, bool>( );
        }

        public void Clear( ) {
            _mapException.Clear( );
            _mapStatus.Clear( );
            _allMapStatus = null;
            _throwOnNonSuccess = false;
        }

        public static bool IsSuccessStatusCode( HttpStatusCode statusCode ) {
            return ( ( int )statusCode >= 200 ) && ( ( int )statusCode <= 299 );
        }

        public RestStatusBuilder StatusIs<TResult>( HttpStatusCode statusCode, Func<string, bool>? condition = null ) {
            return StatusIs( statusCode, typeof( TResult ), condition );
        }

        public RestStatusBuilder StatusIs( HttpStatusCode statusCode, Type type, Func<string, bool>? condition = null ) {
            _mapStatus.AddResultMap( statusCode, condition, type );
            return this;
        }

        public RestStatusBuilder StatusIn( Type type, Func<string, bool>? condition = null, params HttpStatusCode[ ] statusCodes ) {
            statusCodes = statusCodes.Distinct( ).ToArray( );
            foreach ( var statusCode in statusCodes )
                StatusIs( statusCode, type, condition );

            return this;
        }

        public RestStatusBuilder StatusIs<TResult>( Func<string, bool>? condition = null, params HttpStatusCode[ ] statusCodes ) {
            return StatusIn( typeof( TResult ), condition, statusCodes );
        }

        public RestStatusBuilder ThrownOn( Func<string, bool>? condition = null, params HttpStatusCode[ ] statusCodes ) {
            foreach ( var statusCode in statusCodes )
                _mapStatus.AddExceptionMap( statusCode, condition );

            return this;
        }

        public RestStatusBuilder AnyStatus( Type type ) {
            _allMapStatus = type;
            return this;
        }

        public RestStatusBuilder AnyStatus<TResult>( ) {
            _allMapStatus = typeof( TResult );
            return this;
        }

        public RestStatusBuilder ThrowExceptions( Action action ) {
            action.Invoke( );
            return this;
        }

        public RestStatusBuilder NonSuccessStatusCodeThrowsException( bool shouldThrowException ) {
            _throwOnNonSuccess = shouldThrowException;
            return this;
        }

        public bool ContainsStatus( HttpStatusCode statusCode, string content, out Type type ) {
            return _mapStatus.GetResultMap( statusCode, content, out type ) || _allMapStatus is not null;
        }

        public bool ContainsException( HttpStatusCode statusCode, string content ) {
            return _mapStatus.GetExceptiontMap( statusCode, content );
        }

        public bool ShouldThrowException( HttpStatusCode statusCode, string content ) {
            var shouldThrow = _throwOnNonSuccess && !IsSuccessStatusCode( statusCode );
            shouldThrow = ContainsException( statusCode, content );

            return shouldThrow;
        }
    }
}