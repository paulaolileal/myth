using Myth.Exceptions;
using Myth.ViewModels.Errors;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Myth.Extensions {

    public static class ExceptionExtension {

        public static async Task<Exception> ThrowExceptionAsync( this HttpResponseMessage request, Exception exception = null ) {
            if ( request.Headers.Any( header => header.Key.ToLowerInvariant( ) == "x-harpy-version" ) ) {
                try {
                    switch ( request.StatusCode ) {
                        case HttpStatusCode.UnprocessableEntity: {
                            var validation = await request.GetResponse<ValidationResponse>( );
                            return new ValidationException( validation?.Errors, "Some validation errors were found" );
                        }
                        case HttpStatusCode.InternalServerError: {
                            var error = await request.GetResponse<ExeceptionResponse>( );
                            return new ServerException( error.Message, error.StackTrace );
                        }
                        case HttpStatusCode.Unauthorized: {
                            return new RequestException( request.StatusCode, request.RequestMessage.RequestUri, await request.Content.ReadAsStringAsync( ), "Not autorized!" );
                        }
                    }
                } catch { }
            }

            return new RequestException( request.StatusCode, request.RequestMessage.RequestUri, await request.Content.ReadAsStringAsync( ), exception?.Message );
        }

        public static Exception ThrowException( this HttpClient client, Exception exception = null ) {
            return new RequestException( null, client.BaseAddress, default, exception.Message );
        }
    }
}