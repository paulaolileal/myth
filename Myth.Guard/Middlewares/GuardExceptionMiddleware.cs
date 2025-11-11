using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Myth.Exceptions;
using Myth.Models;
using System.Text.Json;

namespace Myth.Middlewares {

	/// <summary>
	/// Middleware to handle exceptions globally with configurable mappings
	/// </summary>
	internal sealed class GuardExceptionMiddleware {

		private readonly RequestDelegate _next;
		private readonly GuardOptions _options;
		private readonly ILogger<GuardExceptionMiddleware>? _logger;

		public GuardExceptionMiddleware( RequestDelegate next, GuardOptions options, ILogger<GuardExceptionMiddleware>? logger = null ) {
			_next = next;
			_options = options;
			_logger = logger;
		}

		public async Task InvokeAsync( HttpContext context ) {
			try {
				await _next( context );

			} catch ( Exception ex ) {
				await HandleExceptionAsync( context, ex );
			}
		}

		private async Task HandleExceptionAsync( HttpContext context, Exception exception ) {
			var handler = FindHandler( exception );

			if ( handler == null ) {
				_logger?.LogError( exception, "Unhandled exception with no configured handler" );
				await WriteDefaultErrorResponse( context, exception );

				return;
			}

			try {
				handler.OnBeforeResponse?.Invoke( exception, context );

				var statusCode = handler.StatusCodeResolver( exception );
				var response = handler.ResponseBuilder( exception );

				context.Response.StatusCode = statusCode;
				context.Response.ContentType = "application/json";

				var options = new JsonSerializerOptions {
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
					WriteIndented = false
				};

				await context.Response.WriteAsJsonAsync( response, options );

			} catch ( Exception handlerException ) {
				_logger?.LogError( handlerException, "Error in exception handler for {ExceptionType}", exception.GetType( ).Name );
				await WriteDefaultErrorResponse( context, exception );
			}
		}

		private ExceptionHandler? FindHandler( Exception exception ) {
			if ( exception is ValidationException )
				return GetValidationExceptionHandler( );

			var exceptionType = exception.GetType( );

			if ( _options.ExceptionHandlers.TryGetValue( exceptionType, out var handler ) )
				return handler;

			var handlerType = _options.ExceptionHandlers.Keys
				.Where( t => t.IsAssignableFrom( exceptionType ) )
				.OrderByDescending( t => GetInheritanceDistance( exceptionType, t ) )
				.FirstOrDefault( );

			if ( handlerType != null )
				return _options.ExceptionHandlers[ handlerType ];

			return _options.DefaultHandler;
		}

		private static ExceptionHandler GetValidationExceptionHandler( ) {
			return new ExceptionHandler {
				ExceptionType = typeof( ValidationException ),
				StatusCodeResolver = ex => {
					var validationEx = ( ValidationException )ex;

					return ( int )validationEx.ValidationResult.StatusCode;
				},
				ResponseBuilder = ex => {
					var validationEx = ( ValidationException )ex;

					return new ValidationErrorResponse {
						Code = validationEx.ValidationResult.Errors.Count > 1
							? "MULTIPLE_ERRORS"
							: validationEx.ValidationResult.Errors[ 0 ].Code,
						Errors = [ .. validationEx.ValidationResult.Errors.Select( e => new ErrorDetail {
							Field = e.Field,
							Message = e.Message,
							Code = e.Code
						} ) ]
					};
				}
			};
		}

		private static int GetInheritanceDistance( Type childType, Type parentType ) {
			var distance = 0;
			var currentType = childType;

			while ( currentType != null && currentType != parentType ) {
				distance++;
				currentType = currentType.BaseType;
			}

			return currentType == parentType ? distance : int.MaxValue;
		}

		private static async Task WriteDefaultErrorResponse( HttpContext context, Exception exception ) {
			context.Response.StatusCode = 500;
			context.Response.ContentType = "application/json";

			var response = new {
				error = "An internal error occurred"
			};

			var options = new JsonSerializerOptions {
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				WriteIndented = false
			};

			await context.Response.WriteAsJsonAsync( response, options );
		}
	}
}
