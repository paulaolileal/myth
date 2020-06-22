using Myth.ValueObjects;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Myth.Extensions {

    public static class HttpExtension {

        /// <summary>
        /// Make a get request
        /// </summary>
        /// <returns>Task<HttpResponseMessage></returns>
        public static async Task<HttpResponseMessage> RequestAsync( this HttpClient httpClient, string url, CancellationToken cancellationToken ) {
            HttpResponseMessage request = null;
            Exception exception = null;

            try {
                request = await httpClient.GetAsync( url, cancellationToken );
            } catch ( Exception e ) {
                exception = e;
            } finally {
                if ( request == null )
                    throw httpClient.ThrowExceptionAsync( exception );
                else if ( !request.IsSuccessStatusCode )
                    throw await request.ThrowExceptionAsync( exception );
                else if ( exception != null )
                    throw new Exception( "Error on request!", exception );
            }

            return request;
        }

        /// <summary>
        /// Make a get request with pagination
        /// </summary>
        /// <returns>Task<HttpResponseMessage></returns>
        public static async Task<HttpResponseMessage> RequestAsync( this HttpClient httpClient, string url, Pagination pagination, CancellationToken cancellationToken ) {
            if ( pagination is null )
                pagination = Pagination.Default;

            url = url.Paginate( pagination.PageNumber, pagination.PageSize );

            return await RequestAsync( httpClient, url, cancellationToken );
        }

        /// <summary>
        /// Make a post request
        /// </summary>
        /// <param name="body">Content body</param>
        /// <returns>Task<HttpResponseMessage></returns>
        public static async Task<HttpResponseMessage> RequestAsync( this HttpClient httpClient, string url, object body, CancellationToken cancellationToken ) {
            HttpResponseMessage request = null;
            Exception exception = null;

            try {
                request = await httpClient.PostAsync( url, new StringContent( JsonConvert.SerializeObject( body ), Encoding.UTF8, "application/json" ), cancellationToken );
            } catch ( Exception e ) {
                exception = e;
            } finally {
                if ( request == null )
                    throw httpClient.ThrowExceptionAsync( exception );
                else if ( !request.IsSuccessStatusCode )
                    throw await request.ThrowExceptionAsync( exception );
                else if ( exception != null )
                    throw new Exception( "Error on request!", exception );
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
            return JsonConvert.DeserializeObject<T>( await response.Content.ReadAsStringAsync( ) );
        }

        public static Task<HttpResponseMessage> PostAsync( this HttpClient httpClient, string url, object body, CancellationToken cancellationToken = default ) =>
            httpClient.PostAsync( url, new StringContent( JsonConvert.SerializeObject( body ), Encoding.UTF8, "application/json" ), cancellationToken );

        public static async Task<T> GetResponse<T>( this HttpResponseMessage httpClient ) =>
            JsonConvert.DeserializeObject<T>( await httpClient.Content.ReadAsStringAsync( ) );
    }
}