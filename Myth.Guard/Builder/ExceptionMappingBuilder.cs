using Microsoft.AspNetCore.Http;
using Myth.Models;

namespace Myth.Builder {

	/// <summary>
	/// Fluent builder for configuring exception handling mappings
	/// </summary>
	/// <typeparam name="TException">The exception type to configure</typeparam>
	public sealed class ExceptionMappingBuilder<TException> where TException : Exception {

		private readonly GuardOptions _options;
		private readonly bool _isDefault;
		private int _statusCode = 500;
		private Func<TException, string>? _errorCodeResolver;
		private Func<TException, object>? _responseBuilder;
		private Action<TException, HttpContext>? _onBeforeResponse;

		internal ExceptionMappingBuilder( GuardOptions options, bool isDefault = false ) {
			_options = options;
			_isDefault = isDefault;
		}

		/// <summary>
		/// Sets the HTTP status code for this exception type
		/// </summary>
		/// <param name="statusCode">The HTTP status code</param>
		public ExceptionMappingBuilder<TException> WithStatusCode( int statusCode ) {
			_statusCode = statusCode;

			return this;
		}

		/// <summary>
		/// Sets a dynamic HTTP status code resolver for this exception type
		/// </summary>
		/// <param name="statusCodeResolver">Function to resolve status code from exception</param>
		public ExceptionMappingBuilder<TException> WithStatusCode( Func<TException, int> statusCodeResolver ) {
			_statusCode = 0;
			_errorCodeResolver = ex => statusCodeResolver( ex ).ToString( );

			return this;
		}

		/// <summary>
		/// Sets the error code for this exception type
		/// </summary>
		/// <param name="errorCode">The error code string</param>
		public ExceptionMappingBuilder<TException> WithErrorCode( string errorCode ) {
			_errorCodeResolver = _ => errorCode;

			return this;
		}

		/// <summary>
		/// Sets a dynamic error code resolver for this exception type
		/// </summary>
		/// <param name="errorCodeResolver">Function to resolve error code from exception</param>
		public ExceptionMappingBuilder<TException> WithErrorCode( Func<TException, string> errorCodeResolver ) {
			_errorCodeResolver = errorCodeResolver;

			return this;
		}

		/// <summary>
		/// Sets the response builder for this exception type
		/// </summary>
		/// <param name="responseBuilder">Function to build response object from exception</param>
		public ExceptionMappingBuilder<TException> WithResponse( Func<TException, object> responseBuilder ) {
			_responseBuilder = responseBuilder;

			return this;
		}

		/// <summary>
		/// Sets a callback to execute before writing the response
		/// </summary>
		/// <param name="callback">Action to execute with the exception and HTTP context</param>
		public ExceptionMappingBuilder<TException> OnBeforeResponse( Action<TException, HttpContext> callback ) {
			_onBeforeResponse = callback;

			return this;
		}

		/// <summary>
		/// Builds and registers the exception handler
		/// </summary>
		internal ExceptionHandler Build( ) {
			var handler = new ExceptionHandler {
				ExceptionType = typeof( TException ),
				StatusCodeResolver = ex => {
					if ( _statusCode > 0 )
						return _statusCode;

					if ( _errorCodeResolver != null && int.TryParse( _errorCodeResolver( ( TException )ex ), out var code ) )
						return code;

					return 500;
				},
				ErrorCodeResolver = _errorCodeResolver != null
					? ex => _errorCodeResolver( ( TException )ex )
					: null,
				ResponseBuilder = _responseBuilder != null
					? ex => _responseBuilder( ( TException )ex )
					: ex => new { error = ex.Message },
				OnBeforeResponse = _onBeforeResponse != null
					? ( ex, ctx ) => _onBeforeResponse( ( TException )ex, ctx )
					: null
			};

			_options.RegisterHandler( typeof( TException ), handler, _isDefault );

			return handler;
		}

		/// <summary>
		/// Finalizes the configuration and returns to the parent options builder
		/// </summary>
		public GuardOptions And( ) {
			Build( );

			return _options;
		}

		/// <summary>
		/// Implicitly builds the handler when the builder goes out of scope
		/// </summary>
		public static implicit operator GuardOptions( ExceptionMappingBuilder<TException> builder ) {
			builder.Build( );

			return builder._options;
		}
	}
}
