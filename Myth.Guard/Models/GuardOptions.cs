using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Myth.Models;

/// <summary>
/// Configuration options for the Guard middleware exception handling
/// </summary>
public sealed class GuardOptions {

	internal Dictionary<Type, ExceptionHandler> ExceptionHandlers { get; } = new( );

	internal ExceptionHandler? DefaultHandler { get; set; }

	internal IWebHostEnvironment? Environment { get; set; }

	internal List<object> PendingBuilders { get; } = new( );

	/// <summary>
	/// Gets or sets the default HTTP status code for validation errors when not explicitly specified
	/// </summary>
	public HttpStatusCode? DefaultValidationStatusCode { get; set; }

	/// <summary>
	/// Automatically guards against common .NET exceptions with sensible defaults
	/// </summary>
	/// <param name="includeStackTrace">Whether to include formatted stack traces in development mode</param>
	/// <returns>The GuardOptions instance for method chaining</returns>
	public GuardOptions AutoGuardCommonExceptions( bool includeStackTrace = true ) {
		Guard<ArgumentNullException>( )
			.WithStatusCode( 400 )
			.WithErrorCode( "ARGUMENT_NULL" )
			.WithResponse( ex => new {
				error = ex.Message,
				parameter = ( ex as ArgumentNullException )?.ParamName
			} )
			.Build( );

		Guard<ArgumentException>( )
			.WithStatusCode( 400 )
			.WithErrorCode( "ARGUMENT_INVALID" )
			.WithResponse( ex => new {
				error = ex.Message,
				parameter = ( ex as ArgumentException )?.ParamName
			} )
			.Build( );

		Guard<InvalidOperationException>( )
			.WithStatusCode( 409 )
			.WithErrorCode( "INVALID_OPERATION" )
			.WithResponse( ex => new {
				error = ex.Message
			} )
			.Build( );

		Guard<UnauthorizedAccessException>( )
			.WithStatusCode( 403 )
			.WithErrorCode( "FORBIDDEN" )
			.WithResponse( ex => new {
				error = "Access denied"
			} )
			.Build( );

		Guard<NotImplementedException>( )
			.WithStatusCode( 501 )
			.WithErrorCode( "NOT_IMPLEMENTED" )
			.WithResponse( ex => new {
				error = "This feature is not implemented"
			} )
			.Build( );

		Guard<TimeoutException>( )
			.WithStatusCode( 408 )
			.WithErrorCode( "TIMEOUT" )
			.WithResponse( ex => new {
				error = "The operation timed out"
			} )
			.Build( );

		GuardDefaultException( )
			.WithStatusCode( 500 )
			.WithErrorCode( "INTERNAL_ERROR" )
			.WithResponse( ex => {
				var isDevelopment = Environment?.IsDevelopment( ) ?? false;

				return new {
					error = isDevelopment
						? ex.Message
						: "An internal error occurred",
					trace = isDevelopment && includeStackTrace
						? FormatStackTrace( ex.StackTrace )
						: null
				};
			} )
			.Build( );

		return this;
	}

	/// <summary>
	/// Guards against a specific exception type with a handler configuration
	/// </summary>
	public Builder.ExceptionMappingBuilder<TException> Guard<TException>( ) where TException : Exception {
		return new Builder.ExceptionMappingBuilder<TException>( this );
	}

	/// <summary>
	/// Guards with a default handler for unhandled exceptions
	/// </summary>
	public Builder.ExceptionMappingBuilder<Exception> GuardDefaultException( ) {
		return new Builder.ExceptionMappingBuilder<Exception>( this, isDefault: true );
	}

	/// <summary>
	/// Sets the default HTTP status code for validation errors when not explicitly specified
	/// </summary>
	/// <param name="statusCode">The HTTP status code to use as default for validation errors</param>
	/// <returns>The GuardOptions instance for method chaining</returns>
	public GuardOptions UseDefaultStatusCode( HttpStatusCode statusCode ) {
		DefaultValidationStatusCode = statusCode;

		return this;
	}

	/// <summary>
	/// Sets the default HTTP status code for validation errors when not explicitly specified
	/// </summary>
	/// <param name="statusCode">The HTTP status code as integer to use as default for validation errors</param>
	/// <returns>The GuardOptions instance for method chaining</returns>
	public GuardOptions UseDefaultStatusCode( int statusCode ) {
		DefaultValidationStatusCode = ( HttpStatusCode )statusCode;

		return this;
	}


	internal void RegisterHandler( Type exceptionType, ExceptionHandler handler, bool isDefault = false ) {
		if ( isDefault )
			DefaultHandler = handler;
		else
			ExceptionHandlers[ exceptionType ] = handler;
	}

	internal void AddPendingBuilder( object builder ) {
		PendingBuilders.Add( builder );
	}

	/// <summary>
	/// Finalizes all pending exception mapping builders
	/// </summary>
	public GuardOptions FinalizePendingBuilders( ) {
		foreach ( var builder in PendingBuilders.ToList( ) ) {
			if ( builder.GetType( ).GetMethod( "ForceBuild", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic ) is { } buildMethod ) {
				buildMethod.Invoke( builder, null );
			}
		}

		PendingBuilders.Clear( );

		return this;
	}

	private static ICollection<string>? FormatStackTrace( string? stackTrace ) {
		if ( string.IsNullOrWhiteSpace( stackTrace ) )
			return null;

		var lines = stackTrace
			.Split( [ "\r\n", "\n" ], StringSplitOptions.RemoveEmptyEntries )
			.Select( line => line.Trim( ) )
			.ToArray( ) ?? [ ];

		return lines;
	}
}
