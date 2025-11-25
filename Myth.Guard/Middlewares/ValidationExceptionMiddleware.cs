using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Myth.Exceptions;
using Myth.Models;

namespace Myth.Middlewares;

/// <summary>
/// Middleware to handle validation exceptions
/// </summary>
internal sealed class ValidationExceptionMiddleware( RequestDelegate next ) {
	private readonly RequestDelegate _next = next;

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
				Code = e.Code,
				Options = e.Options
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
