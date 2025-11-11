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

	/// <summary>
	/// Automatically maps common .NET exceptions with sensible defaults
	/// </summary>
	/// <param name="includeStackTrace">Whether to include formatted stack traces in development mode</param>
	/// <returns>The GuardOptions instance for method chaining</returns>
	public GuardOptions AutoMapCommonExceptions( bool includeStackTrace = true ) {
		MapException<ArgumentNullException>( )
			.WithStatusCode( 400 )
			.WithErrorCode( "ARGUMENT_NULL" )
			.WithResponse( ex => new {
				error = ex.Message,
				parameter = ( ex as ArgumentNullException )?.ParamName
			} )
			.Build( );

		MapException<ArgumentException>( )
			.WithStatusCode( 400 )
			.WithErrorCode( "ARGUMENT_INVALID" )
			.WithResponse( ex => new {
				error = ex.Message,
				parameter = ( ex as ArgumentException )?.ParamName
			} )
			.Build( );

		MapException<InvalidOperationException>( )
			.WithStatusCode( 409 )
			.WithErrorCode( "INVALID_OPERATION" )
			.WithResponse( ex => new {
				error = ex.Message
			} )
			.Build( );

		MapException<UnauthorizedAccessException>( )
			.WithStatusCode( 403 )
			.WithErrorCode( "FORBIDDEN" )
			.WithResponse( ex => new {
				error = "Access denied"
			} )
			.Build( );

		MapException<NotImplementedException>( )
			.WithStatusCode( 501 )
			.WithErrorCode( "NOT_IMPLEMENTED" )
			.WithResponse( ex => new {
				error = "This feature is not implemented"
			} )
			.Build( );

		MapException<TimeoutException>( )
			.WithStatusCode( 408 )
			.WithErrorCode( "TIMEOUT" )
			.WithResponse( ex => new {
				error = "The operation timed out"
			} )
			.Build( );

		MapDefaultException( )
			.WithStatusCode( 500 )
			.WithErrorCode( "INTERNAL_ERROR" )
			.WithResponse( ex => {
				var isDevelopment = Environment?.IsDevelopment( ) ?? false;

				return new {
					error = isDevelopment ? ex.Message : "An internal error occurred",
					trace = isDevelopment && includeStackTrace ? FormatStackTrace( ex.StackTrace ) : null
				};
			} )
			.Build( );

		return this;
	}

	/// <summary>
	/// Maps a specific exception type to a handler configuration
	/// </summary>
	public Builder.ExceptionMappingBuilder<TException> MapException<TException>( ) where TException : Exception {
		return new Builder.ExceptionMappingBuilder<TException>( this );
	}

	/// <summary>
	/// Maps the default handler for unhandled exceptions
	/// </summary>
	public Builder.ExceptionMappingBuilder<Exception> MapDefaultException( ) {
		return new Builder.ExceptionMappingBuilder<Exception>( this, isDefault: true );
	}

	internal void RegisterHandler( Type exceptionType, ExceptionHandler handler, bool isDefault = false ) {
		if ( isDefault )
			DefaultHandler = handler;
		else
			ExceptionHandlers[ exceptionType ] = handler;
	}

	private static string? FormatStackTrace( string? stackTrace ) {
		if ( string.IsNullOrWhiteSpace( stackTrace ) )
			return null;

		var lines = stackTrace.Split( new[ ] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries );

		var formatted = string.Join( "\n", lines.Select( line => {
			var trimmed = line.Trim( );

			if ( trimmed.StartsWith( "at " ) )
				return $"  {trimmed}";

			return trimmed;
		} ) );

		return formatted;
	}
}
