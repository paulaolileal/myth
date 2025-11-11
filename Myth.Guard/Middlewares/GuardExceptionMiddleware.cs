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
			if ( exception is ValidationException validationException ) {
				await HandleValidationExceptionAsync( context, validationException );

				return;
			}

			var handler = FindHandler( exception );

			if ( handler == null ) {
				_logger?.LogWarning( "Exception of type {ExceptionType} not handled by Guard middleware, re-throwing", exception.GetType( ).Name );
				throw exception;
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
				throw exception;
			}
		}

		private static async Task HandleValidationExceptionAsync( HttpContext context, ValidationException exception ) {
			var response = new ValidationErrorResponse {
				Code = exception.ValidationResult.Errors.Count > 1
					? "MULTIPLE_ERRORS"
					: exception.ValidationResult.Errors[ 0 ].Code,
				Errors = [ .. exception.ValidationResult.Errors.Select( e => new ErrorDetail {
					Field = e.Field,
					Message = e.Message,
					Code = e.Code
				} ) ]
			};

			context.Response.StatusCode = ( int )exception.ValidationResult.StatusCode;
			context.Response.ContentType = "application/json";

			var options = new JsonSerializerOptions {
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				WriteIndented = false
			};

			await context.Response.WriteAsJsonAsync( response, options );
		}

		private ExceptionHandler? FindHandler( Exception exception ) {
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

		private static int GetInheritanceDistance( Type childType, Type parentType ) {
			var distance = 0;
			var currentType = childType;

			while ( currentType != null && currentType != parentType ) {
				distance++;
				currentType = currentType.BaseType;
			}

			return currentType == parentType ? distance : int.MaxValue;
		}
	}
}
