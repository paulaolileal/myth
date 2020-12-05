using Myth.ValueObjects.RequestObjects;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Myth.Extensions {

    public static class HttpExtension {

        private static async Task ThrowExceptionAsync( HttpClient httpClient, HttpResponseMessage httpResponse, Exception exception ) {
            if ( httpResponse == null )
                throw httpClient.ThrowException( exception );
            else if ( !httpResponse.IsSuccessStatusCode )
                throw await httpResponse.ThrowExceptionAsync( exception );
            else if ( exception != null )
                throw new Exception( "Error on request!", exception );
        }

        /// <summary>
        /// Make a get request
        /// </summary>
        /// <returns>Task<HttpResponseMessage></returns>
        public static async Task<HttpResponseMessage> RequestAsync( this HttpClient httpClient, string url, CancellationToken cancellationToken = default ) {
            HttpResponseMessage request = null;

            try {
                request = await httpClient.GetAsync( url, cancellationToken );
            } catch ( Exception e ) {
                await ThrowExceptionAsync( httpClient, request, e );
            }

            return request;
        }

        /// <summary>
        /// Make a get request with pagination
        /// </summary>
        /// <returns>Task<HttpResponseMessage></returns>
        public static async Task<HttpResponseMessage> RequestAsync<TViewModel>( this HttpClient httpClient, string url, Odata<TViewModel> odata, CancellationToken cancellationToken = default ) {
            if ( odata == null )
                odata = new Odata<TViewModel>( );

            return await RequestAsync( httpClient, odata.Build( url ), cancellationToken );
        }

        /// <summary>
        /// Make a post request
        /// </summary>
        /// <param name="body">Content body</param>
        /// <returns>Task<HttpResponseMessage></returns>
        public static async Task<HttpResponseMessage> RequestAsync<TRequest>( this HttpClient httpClient, string url, TRequest body, CancellationToken cancellationToken = default ) {
            HttpResponseMessage request = null;

            try {
                request = await httpClient.PostAsync( url, new StringContent( JsonConvert.SerializeObject( body ), Encoding.UTF8, "application/json" ), cancellationToken );
            } catch ( Exception exception ) {
                await ThrowExceptionAsync( httpClient, request, exception );
            }

            return request;
        }

        public static async Task<HttpResponseMessage> RequestAsync<TRequest, TViewModel>( this HttpClient httpClient, string url, Odata<TViewModel> odata, TRequest body, CancellationToken cancellationToken = default ) {
            HttpResponseMessage request = null;

            if ( odata == null )
                odata = new Odata<TViewModel>( );

            try {
                request = await httpClient.PostAsync( odata.Build( url ), new StringContent( JsonConvert.SerializeObject( body ), Encoding.UTF8, "application/json" ), cancellationToken );
            } catch ( Exception exception ) {
                await ThrowExceptionAsync( httpClient, request, exception );
            }

            return request;
        }

        /// <summary>
        /// Deserialize the response body
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <returns>Expected type</returns>
        public static async Task<T> ThenGet<T>( this Task<HttpResponseMessage> httpResponse ) {
            var response = await httpResponse;
            return await response.GetResponse<T>( );
        }

        [Obsolete]
        public static Task<HttpResponseMessage> PostAsync( this HttpClient httpClient, string url, object body, CancellationToken cancellationToken = default ) =>
            httpClient.PostAsync( url, new StringContent( JsonConvert.SerializeObject( body ), Encoding.UTF8, "application/json" ), cancellationToken );

        public static async Task<T> GetResponse<T>( this HttpResponseMessage httpClient ) {
            try {
                return JsonConvert.DeserializeObject<T>( await httpClient.Content.ReadAsStringAsync( ) );
            } catch ( Exception ) {
                throw new Exception( $"Error: the content type is different of {typeof( T )}" );
            }
        }
    }
}