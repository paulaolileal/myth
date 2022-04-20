using System.Net;

namespace Myth.Rest {

    public class RestStatusBuilder {
        private readonly IDictionary<HttpStatusCode, Type> _mapStatus;
        private readonly IDictionary<HttpStatusCode, bool> _mapException;
        private Type _allMapStatus;
        private bool _throwOnNonSuccess;

        public RestStatusBuilder( ) {
            _mapStatus = new Dictionary<HttpStatusCode, Type>( );
            _mapException = new Dictionary<HttpStatusCode, bool>( );
        }

        public static bool IsSuccessStatusCode( HttpStatusCode statusCode ) {
            return ( ( int )statusCode >= 200 ) && ( ( int )statusCode <= 299 );
        }

        public RestStatusBuilder StatusIs<TResult>( HttpStatusCode statusCode, bool shouldThrowException = false ) {
            _mapStatus.TryAdd( statusCode, typeof( TResult ) );
            _mapException.TryAdd( statusCode, shouldThrowException );
            return this;
        }

        public RestStatusBuilder StatusIs( HttpStatusCode statusCode, Type type, bool shouldThrowException = false ) {
            _mapStatus.TryAdd( statusCode, type );
            _mapException.TryAdd( statusCode, shouldThrowException );
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

        public bool ContainsStatus( HttpStatusCode statusCode ) {
            return _mapStatus.ContainsKey( statusCode ) || _allMapStatus is not null;
        }

        public bool ContainsException( HttpStatusCode statusCode ) {
            return _mapException.ContainsKey( statusCode );
        }

        public Type GetMappedType( HttpStatusCode statusCode ) {
            var result = _allMapStatus;

            if ( _mapStatus.ContainsKey( statusCode ) )
                result = _mapStatus[ statusCode ];

            return result;
        }

        public bool GetMappedException( HttpStatusCode statusCode ) {
            return _mapException[ statusCode ];
        }

        public bool ShouldThrowException( HttpStatusCode statusCode ) {
            var shouldThrow = _throwOnNonSuccess && !IsSuccessStatusCode( statusCode );
            if ( ContainsException( statusCode ) )
                shouldThrow = GetMappedException( statusCode );

            return shouldThrow;
        }
    }
}