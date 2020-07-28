using Myth.Exceptions;
using Myth.ViewModels.Errors;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Myth.Extensions {

    public static class ExceptionExtension {

        public static async Task<Exception> ThrowExceptionAsync( this HttpResponseMessage request, Exception exception = null ) {
            switch ( request.StatusCode ) {
                case HttpStatusCode.UnprocessableEntity: {
                    var validation = await request.GetResponse<ValidationResponse>( );
                    return new ValidationException( validation?.Errors, "Some validation errors were found" );
                }
                case HttpStatusCode.InternalServerError: {
                    var error = await request.GetResponse<ExeceptionResponse>( );
                    return new ServerException( error.Message, error.StackTrace );
                }
                default: {
                    return new RequestException( request.StatusCode, request.RequestMessage.RequestUri, await request.Content.ReadAsStringAsync( ), exception?.Message );
                }
            }
        }

        public static Exception ThrowExceptionAsync( this HttpClient client, Exception exception = null ) {
            return new RequestException( null, client.BaseAddress, default, exception.Message );
        }
    }
}