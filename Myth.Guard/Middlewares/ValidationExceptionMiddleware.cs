using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Myth.Exceptions;
using Myth.Models;
using System.Text.Json;

namespace Myth.Middlewares {

	/// <summary>
	/// Middleware to handle validation exceptions
	/// </summary>
	internal sealed class ValidationExceptionMiddleware {
		private readonly RequestDelegate _next;

		public ValidationExceptionMiddleware( RequestDelegate next ) {
			_next = next;
		}

		public async Task InvokeAsync( HttpContext context ) {
			try {
				await _next( context );
			} catch ( ValidationException ex ) {
				await HandleValidationExceptionAsync( context, ex );
			}
		}

		private static async Task HandleValidationExceptionAsync( HttpContext context, ValidationException exception ) {
			var response = new ValidationErrorResponse {
				Code = exception.ValidationResult.Errors.Count > 1 ? "MULTIPLE_ERRORS" :
					   exception.ValidationResult.Errors[ 0 ].Code,
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
	}
}