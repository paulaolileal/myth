using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using static Myth.Documentations.Settings.SwaggerSettings;

namespace Myth.Documentations.Settings {

	/// <summary>
	/// Configuration settings for Swagger UI authentication requirements and security
	/// </summary>
	/// <remarks>
	/// This class provides configuration options for requiring authentication to access Swagger documentation,
	/// particularly useful in production environments where API documentation should be protected.
	/// </remarks>
	internal class SwaggerAuthenticationSettings {

		/// <summary>
		/// Gets or sets a value indicating whether authentication is required to access Swagger UI
		/// </summary>
		/// <value>
		/// true if authentication is required to access Swagger documentation; otherwise, false.
		/// Default is false for development-friendly setup.
		/// </value>
		public bool RequireAuthentication { get; set; } = false;

		/// <summary>
		/// Gets or sets a value indicating whether to validate tokens against ASP.NET Core authentication system
		/// </summary>
		/// <value>
		/// true to validate authentication tokens server-side; otherwise, false.
		/// Default is false to avoid breaking existing setups.
		/// </value>
		public bool ValidateTokens { get; set; } = false;

		/// <summary>
		/// Gets or sets the list of allowed authentication schemes for Swagger access
		/// </summary>
		/// <value>
		/// A list of authorization types that are accepted for Swagger authentication.
		/// Default includes Bearer, Basic, and ApiKey schemes.
		/// </value>
		public List<AuthorizationType> AllowedSchemes { get; set; } = new( ) {
			AuthorizationType.Bearer,
			AuthorizationType.Basic,
			AuthorizationType.ApiKey
		};

		/// <summary>
		/// Gets or sets custom authentication validation logic
		/// </summary>
		/// <value>
		/// A custom function to validate authentication for Swagger access.
		/// If null, uses default ASP.NET Core authentication validation.
		/// </value>
		public Func<HttpContext, Task<bool>>? CustomAuthenticationValidator { get; set; }

		/// <summary>
		/// Gets or sets the unauthorized response configuration
		/// </summary>
		/// <value>
		/// Configuration for the response when authentication fails.
		/// Default returns a JSON error with 401 status code.
		/// </value>
		public UnauthorizedResponseSettings UnauthorizedResponse { get; set; } = new( );

		/// <summary>
		/// Gets or sets environment-specific authentication requirements
		/// </summary>
		/// <value>
		/// A function that determines if authentication should be required based on the current environment.
		/// Default requires authentication only in production environments.
		/// </value>
		public Func<IHostEnvironment, bool> EnvironmentBasedRequirement { get; set; } = env => env.IsProduction( );

		/// <summary>
		/// Gets or sets bypass paths that don't require authentication
		/// </summary>
		/// <value>
		/// A list of paths that bypass authentication requirements.
		/// Useful for health checks or public documentation sections.
		/// </value>
		public List<string> BypassPaths { get; set; } = new( );

		/// <summary>
		/// Gets or sets a value indicating whether to log authentication attempts
		/// </summary>
		/// <value>
		/// true to log authentication attempts for security monitoring; otherwise, false.
		/// Default is true for security auditing.
		/// </value>
		public bool EnableAuditLogging { get; set; } = true;
	}

	/// <summary>
	/// Configuration for unauthorized response when Swagger authentication fails
	/// </summary>
	/// <remarks>
	/// Provides customization options for the response returned when authentication
	/// is required but not provided or invalid.
	/// </remarks>
	internal class UnauthorizedResponseSettings {

		/// <summary>
		/// Gets or sets the HTTP status code for unauthorized responses
		/// </summary>
		/// <value>
		/// The HTTP status code to return. Default is 401 (Unauthorized).
		/// </value>
		public int StatusCode { get; set; } = 401;

		/// <summary>
		/// Gets or sets the content type for the unauthorized response
		/// </summary>
		/// <value>
		/// The content type of the response. Default is "application/json".
		/// </value>
		public string ContentType { get; set; } = "application/json";

		/// <summary>
		/// Gets or sets the error message for unauthorized access
		/// </summary>
		/// <value>
		/// The error message displayed to users. Default explains authentication requirement.
		/// </value>
		public string Message { get; set; } = "Authentication is required to access API documentation";

		/// <summary>
		/// Gets or sets the error code for unauthorized access
		/// </summary>
		/// <value>
		/// A machine-readable error code. Default is "SWAGGER_AUTH_REQUIRED".
		/// </value>
		public string ErrorCode { get; set; } = "SWAGGER_AUTH_REQUIRED";

		/// <summary>
		/// Gets or sets additional headers to include in unauthorized responses
		/// </summary>
		/// <value>
		/// Dictionary of additional headers to include in the response.
		/// Useful for CORS or security headers.
		/// </value>
		public Dictionary<string, string> AdditionalHeaders { get; set; } = new( );

		/// <summary>
		/// Gets or sets a value indicating whether to include timestamp in error responses
		/// </summary>
		/// <value>
		/// true to include timestamp in error responses; otherwise, false.
		/// Default is true for debugging and auditing.
		/// </value>
		public bool IncludeTimestamp { get; set; } = true;

		/// <summary>
		/// Gets or sets custom response body generation logic
		/// </summary>
		/// <value>
		/// A function to generate custom response body. If null, uses default JSON format.
		/// </value>
		public Func<string>? CustomResponseBodyGenerator { get; set; }
	}
}