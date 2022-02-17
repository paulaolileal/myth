using Myth.Exceptions;
using Myth.Extensions;
using Myth.Models.Rest;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;

namespace Myth.Rest {

    public class RestBuilder {
        protected HttpClient _client;
        protected Exception _exception;
        protected Task<HttpResponseMessage> _responseMessage;

        protected readonly RestConfigBuilder _configBuilder;
        protected readonly RestStatusBuilder _statusBuilder;

        public RestBuilder( ) {
            _client = new HttpClient( );
            _statusBuilder = new( );
            _configBuilder = new( );
        }

        public RestBuilder( HttpClient httpClient ) : this( ) {
            _client = httpClient;
        }

        public RestBuilder( RestConfigBuilder configBuilder ) : this( ) {
            _configBuilder = configBuilder;
        }

        public RestBuilder( HttpClient httpClient, RestConfigBuilder configBuilder ) : this( ) {
            _client = httpClient;
            _configBuilder = configBuilder;
        }

        public static RestBuilder Create( HttpClient httpClient, Action<RestConfigBuilder> configurationBuilder ) {
            var configBuilder = new RestConfigBuilder( );
            configurationBuilder.Invoke( configBuilder );

            return new( httpClient, configBuilder );
        }

        public static RestBuilder Create( Action<RestConfigBuilder> configurationBuilder ) {
            var configBuilder = new RestConfigBuilder( );
            configurationBuilder.Invoke( configBuilder );

            return new( configBuilder );
        }

        public static RestBuilder Create( HttpClient httpClient ) => new( httpClient );

        public RestBuilder DoGet( string url, CancellationToken cancellationToken = default ) {
            try {
                PreRequestSettings( );
                _responseMessage = _client.GetAsync( url, cancellationToken );
            } catch ( Exception exception ) {
                _exception = exception;
            }
            return this;
        }

        public RestBuilder DoPost<TBody>( string url, TBody body, CancellationToken cancellationToken = default ) {
            try {
                PreRequestSettings( );
                _responseMessage = _client.PostAsync( url, body.ToHttpContent( ), cancellationToken );
            } catch ( Exception exception ) {
                _exception = exception;
            }
            return this;
        }

        public RestBuilder DoPut<TBody>( string url, TBody body, CancellationToken cancellationToken = default ) {
            try {
                PreRequestSettings( );
                _responseMessage = _client.PutAsync( url, body.ToHttpContent( ), cancellationToken );
            } catch ( Exception exception ) {
                _exception = exception;
            }
            return this;
        }

        public RestBuilder DoDelete( string url, CancellationToken cancellationToken = default ) {
            try {
                PreRequestSettings( );
                _responseMessage = _client.DeleteAsync( url, cancellationToken );
            } catch ( Exception exception ) {
                _exception = exception;
            }
            return this;
        }

        public RestBuilder DoPatch<TBody>( string url, TBody body, CancellationToken cancellationToken = default ) {
            try {
                PreRequestSettings( );
                _responseMessage = _client.PatchAsync( url, body.ToHttpContent( ), cancellationToken );
            } catch ( Exception exception ) {
                _exception = exception;
            }
            return this;
        }

        public RestBuilder When( Action<RestStatusBuilder> statusConfiguration ) {
            statusConfiguration.Invoke( _statusBuilder );
            return this;
        }

        public async Task<TResult> BuildResultAsync<TResult>( ) {
            try {
                var message = await _responseMessage;
                var content = await message.Content.ReadAsStringAsync( );

                TResult responseBody;
                try {
                    responseBody = JsonConvert.DeserializeObject<TResult>( content );
                } catch ( Exception exception ) {
                    throw new MapContentException( typeof( TResult ), content, exception );
                }

                return responseBody;
            } catch ( Exception exception ) {
                _exception = exception;
                throw _exception;
            }
        }

        public async Task<RestResponse> BuildResultAsync( ) {
            try {
                var message = await _responseMessage;
                var content = await message.Content.ReadAsStringAsync( );

                Type? type = null;
                object? responseBody = null;

                if ( _statusBuilder.ContainsStatus( message.StatusCode ) ) {
                    type = _statusBuilder.GetMappedType( message.StatusCode );

                    try {
                        responseBody = JsonConvert.DeserializeObject( content, type );
                    } catch ( Exception exception ) {
                        throw new MapContentException( type, content, exception );
                    }
                }

                if ( _statusBuilder.ShouldThrowException( message.StatusCode ) )
                    throw new NonSuccessException(
                        message.StatusCode,
                        message.RequestMessage!.RequestUri!,
                        message.RequestMessage.Method,
                        content,
                        type,
                        responseBody );

                return new RestResponse(
                    message.StatusCode,
                    message.RequestMessage!.RequestUri!,
                    message.RequestMessage.Method,
                    content,
                    type,
                    responseBody );
            } catch ( Exception exception ) {
                _exception = exception;
                throw _exception;
            }
        }

        private void PreRequestSettings( ) {
            var baseAddress = _configBuilder._baseUrl;
            if ( !string.IsNullOrEmpty( baseAddress ) &&
               ( _client.BaseAddress is not null &&
               !_client.BaseAddress.ToString( ).Contains( baseAddress ) ) )
                _client.BaseAddress = new Uri(baseAddress);

            if ( _configBuilder.AuthorizationIsSetted )
                _client.DefaultRequestHeaders.Authorization = _configBuilder._authorizationHeader;

            if ( !string.IsNullOrEmpty( _configBuilder._acceptContentType ) )
                _client
                    .DefaultRequestHeaders
                    .Accept
                    .Add( new MediaTypeWithQualityHeaderValue( _configBuilder._acceptContentType ) );

            if ( _configBuilder._customHeaders.Any( ) )
                foreach ( var header in _configBuilder._customHeaders ) {
                    _client
                        .DefaultRequestHeaders.Add( header.Key, header.Value );
                }
        }
    }

    public class Test {

        public async Task TesteAsync( ) {
            var client = await RestBuilder
                .Create( x => x
                    .WithBaseUrl( "" )
                    .WithBearerAuthorization( "" ) )
                .DoGet( "testes" )
                .When( config => config
                    .ThrowExceptions( ( ) => Console.WriteLine( ) )
                    .StatusIs<Test>( HttpStatusCode.OK )
                    .StatusIs( HttpStatusCode.BadGateway, typeof( Test ) ) )
                .BuildResultAsync( );
        }
    }
}