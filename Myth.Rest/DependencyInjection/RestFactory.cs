using Myth.Rest;

namespace Myth.DependencyInjection {

    public class RestFactory {
        private IDictionary<string, RestBuilder> _clients;

        public RestFactory( ) {
            _clients = new Dictionary<string, RestBuilder>( );
        }

        public void Add( string name, RestBuilder builder ) => _clients.TryAdd( name, builder );

        public RestBuilder Get( string name ) => _clients[ name ];
    }
}