using Myth.Exceptions;
using Myth.Extensions;
using Myth.Models.Rest;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace Myth.Rest {

    public class RestBuilder : IDisposable {
        protected HttpClient _client;
        protected Exception? _exception;
        protected Task<HttpResponseMessage>? _responseMessage;
        protected Func<CancellationToken, Task<HttpResponseMessage>>? _request;

        protected RestConfigBuilder _configBuilder;
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

        public static RestBuilder Create( Action<RestConfigBuilder>? configurationBuilder = null ) {
            var configBuilder = new RestConfigBuilder( );

            if ( configurationBuilder != null )
                configurationBuilder.Invoke( configBuilder );

            return new( configBuilder );
        }

        public static RestBuilder Create( HttpClient httpClient ) => new( httpClient );

        public RestBuilder DoGet( string url ) {
            try {
                PreRequestSettings( );

                _request = async ( CancellationToken cancellationToken ) => await _client.GetAsync( url, cancellationToken );
            } catch ( Exception exception ) {
                _exception = exception;
            }
            return this;
        }

        public RestBuilder DoPost<TBody>( string url, TBody? body = default ) {
            try {
                ArgumentNullException.ThrowIfNull( body, nameof( body ) );
                PreRequestSettings( );

                var request = ToHttpContent( body ) ?? null;

                _request = async ( CancellationToken cancellationToken ) => await _client.PostAsync( url, request, cancellationToken );
            } catch ( Exception exception ) {
                _exception = exception;
            }
            return this;
        }

        public RestBuilder DoPut<TBody>( string url, TBody? body = default ) {
            try {
                ArgumentNullException.ThrowIfNull( body, nameof( body ) );
                PreRequestSettings( );

                var request = ToHttpContent( body ) ?? null;

                _request = async ( CancellationToken cancellationToken ) => await _client.PutAsync( url, request, cancellationToken );
            } catch ( Exception exception ) {
                _exception = exception;
            }
            return this;
        }

        public RestBuilder DoDelete( string url ) {
            try {
                PreRequestSettings( );

                _request = async ( CancellationToken cancellationToken ) => await _client.DeleteAsync( url, cancellationToken );
            } catch ( Exception exception ) {
                _exception = exception;
            }
            return this;
        }

        public RestBuilder DoPatch<TBody>( string url, TBody? body = default ) {
            try {
                ArgumentNullException.ThrowIfNull( body, nameof( body ) );
                PreRequestSettings( );

                var request = ToHttpContent( body ) ?? null;

                _request = async ( CancellationToken cancellationToken ) => await _client.PatchAsync( url, request, cancellationToken );
            } catch ( Exception exception ) {
                _exception = exception;
            }
            return this;
        }

        public RestBuilder WithConfiguration( Action<RestConfigBuilder>? configurationBuilder ) {
            if ( _configBuilder is null )
                _configBuilder = new RestConfigBuilder( );

            if ( configurationBuilder != null )
                configurationBuilder.Invoke( _configBuilder );

            return this;
        }

        public RestBuilder When( Action<RestStatusBuilder> statusConfiguration, bool clearOldSettings = true ) {
            if ( clearOldSettings )
                _statusBuilder.Clear( );

            statusConfiguration.Invoke( _statusBuilder );
            return this;
        }

        public async Task<RestResponse> BuildResultAsync<TResult>( CancellationToken cancellationToken = default ) => await BuildResultAsync( typeof( TResult ), cancellationToken );

        public async Task<RestResponse> BuildResultAsync( CancellationToken cancellationToken = default ) => await BuildResultAsync( null, cancellationToken );

        private async Task<RestResponse> BuildResultAsync( Type? responseType = null, CancellationToken cancellationToken = default ) {
            try {
                var restResponse = await ProcessResponseAsync( responseType, cancellationToken );

                if ( _statusBuilder.ShouldThrowException( restResponse.StatusCode, restResponse.RawMessage ) )
                    throw new NonSuccessException( restResponse );

                return restResponse;
            } catch ( Exception exception ) {
                _exception = exception;
                throw _exception;
            }
        }

        /// <summary>
        /// Downloads a file
        /// </summary>
        /// <param name="url">Url of location</param>
        /// <param name="destinationPath">The full path including the file name</param>
        /// <param name="replaceExistingFile">If remove old file and create a new one</param>
        /// <returns></returns>
        /// <exception cref="DownloadException"></exception>
        public async Task DownloadFileAsync( string url, string destinationPath, bool replaceExistingFile = false, CancellationToken cancellationToken = default ) {
            try {
                PreRequestSettings( );
                if ( File.Exists( destinationPath ) && !replaceExistingFile )
                    throw new DownloadException( "File already exists!", destinationPath, url );

                if ( replaceExistingFile )
                    File.Delete( destinationPath );

                var fileBytes = await _client.GetByteArrayAsync( url, cancellationToken: cancellationToken );
                await File.WriteAllBytesAsync( destinationPath, fileBytes, cancellationToken );
            } catch ( Exception exception ) {
                _exception = exception;
                throw _exception;
            }
        }

        private void PreRequestSettings( ) {
            var baseAddress = _configBuilder._baseUrl;
            if ( !string.IsNullOrEmpty( baseAddress ) &&
               ( _client.BaseAddress is null ||
               ( _client.BaseAddress is not null &&
               !_client.BaseAddress.ToString( ).Contains( baseAddress ) ) ) )
                _client.BaseAddress = new Uri( baseAddress );

            if ( _configBuilder.AuthorizationIsSetted )
                _client.DefaultRequestHeaders.Authorization = _configBuilder._authorizationHeader;

            if ( !string.IsNullOrEmpty( _configBuilder._acceptContentType ) )
                _client
                    .DefaultRequestHeaders
                    .Accept
                    .Add( new MediaTypeWithQualityHeaderValue( _configBuilder._acceptContentType ) );

            if ( _configBuilder._customHeaders.Any( ) )
                foreach ( var header in _configBuilder._customHeaders ) {
                    if ( !_client.DefaultRequestHeaders.Contains( header.Key ) )
                        _client
                            .DefaultRequestHeaders.Add( header.Key, header.Value );
                }
        }

        private async Task<RestResponse> ProcessResponseAsync( Type? responseType = null, CancellationToken cancellationToken = default ) {
            if ( _request is null )
                throw new RequestException( );

            var requestTime = new Stopwatch( );

            requestTime.Start( );

            var message = await _request.Invoke( cancellationToken );

            requestTime.Stop( );

            var content = await message.Content.ReadAsStringAsync( cancellationToken );
            object? responseBody = null;

            if ( responseType is null && _statusBuilder.ContainsStatus( message.StatusCode, content, out var type ) )
                responseType = type;

            if ( responseType is not null && !string.IsNullOrEmpty( content ) ) {
                try {
                    responseBody = content.FromJson( responseType, _configBuilder._deserializationCaseStrategy );
                } catch ( Exception exception ) {
                    throw new MapContentException( responseType, content, exception );
                }
            }

            var restResponse = new RestResponse(
                message.StatusCode,
                message.RequestMessage!.RequestUri!,
                message.RequestMessage.Method,
                content,
                responseType,
                responseBody,
                requestTime.Elapsed );

            return restResponse;
        }

        private HttpContent ToHttpContent<TBody>( TBody body ) {
            HttpContent request;
            if ( body is HttpContent content )
                request = content;
            else
                request = body.ToHttpContent( _configBuilder._serializationCaseStrategy );
            return request;
        }

        public void Dispose( ) {
            if ( _client is not null )
                _client.Dispose( );

            if ( _responseMessage is not null )
                _responseMessage.Dispose( );
        }
    }
}