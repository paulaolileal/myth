using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Myth.Exceptions;
using Myth.Models;

namespace Myth.Middlewares;

/// <summary>
/// Middleware to handle exceptions globally with configurable mappings
/// </summary>
internal sealed class GuardExceptionMiddleware( RequestDelegate next, GuardOptions options, ILogger<GuardExceptionMiddleware>? logger = null ) {

	private readonly RequestDelegate _next = next;
	private readonly GuardOptions _options = options;
	private readonly ILogger<GuardExceptionMiddleware>? _logger = logger;

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
		// Group errors by field for RFC 7807 format
		var errorsByField = exception.ValidationResult.Errors
			.GroupBy( e => e.Field )
			.ToDictionary(
				g => g.Key,
				g => g.Select( e => e.Message ).ToArray( )
			);

		var problemDetails = new ValidationProblemDetails( errorsByField ) {
			Type = "https://github.com/paulaolileal/myth/blob/main/docs/errors/validation.md",
			Title = "One or more validation errors occurred",
			Status = ( int )exception.ValidationResult.StatusCode,
			Instance = context.Request.Path
		};

		// Add traceId for correlation
		if ( context.TraceIdentifier != null ) {
			problemDetails.Extensions[ "traceId" ] = context.TraceIdentifier;
		}

		// Add options for fields that have them (enum/constant validation)
		var fieldsWithOptions = exception.ValidationResult.Errors
			.Where( e => e.Options != null && e.Options.Count > 0 )
			.GroupBy( e => e.Field )
			.ToDictionary(
				g => g.Key,
				g => g.First( ).Options
			);

		if ( fieldsWithOptions.Count > 0 ) {
			problemDetails.Extensions[ "options" ] = fieldsWithOptions;
		}

		context.Response.StatusCode = ( int )exception.ValidationResult.StatusCode;
		context.Response.ContentType = "application/problem+json";

		var options = new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			WriteIndented = false,
			DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
		};

		var json = JsonSerializer.Serialize( problemDetails, options );
		await context.Response.WriteAsync( json );
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
