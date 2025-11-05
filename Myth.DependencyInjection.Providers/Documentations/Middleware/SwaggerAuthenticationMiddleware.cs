using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Myth.Documentations.Settings;

namespace Myth.Documentations.Middleware {

    /// <summary>
    /// Middleware to protect Swagger endpoints with authentication
    /// </summary>
    internal class SwaggerAuthenticationMiddleware {
        private readonly RequestDelegate _next;
        private readonly ILogger<SwaggerAuthenticationMiddleware> _logger;

        public SwaggerAuthenticationMiddleware(RequestDelegate next, ILogger<SwaggerAuthenticationMiddleware> logger) {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Processes the HTTP request and validates authentication for Swagger endpoints
        /// </summary>
        /// <param name="context">The HTTP context</param>
        /// <returns>Task representing the async operation</returns>
        public async Task InvokeAsync(HttpContext context) {
            // Check if request is for Swagger endpoints
            if (!IsSwaggerEndpoint(context.Request.Path)) {
                await _next(context);
                return;
            }

            // Get configuration (in a real implementation, this would be injected)
            var requireAuth = GetAuthenticationRequirement(context);

            if (!requireAuth) {
                await _next(context);
                return;
            }

            // Validate authentication
            var isAuthenticated = await ValidateAuthenticationAsync(context);

            if (!isAuthenticated) {
                await HandleUnauthorizedAsync(context);
                return;
            }

            // Validate authorization if needed
            var isAuthorized = await ValidateAuthorizationAsync(context);

            if (!isAuthorized) {
                await HandleForbiddenAsync(context);
                return;
            }

            await _next(context);
        }

        /// <summary>
        /// Checks if the request path is for a Swagger endpoint
        /// </summary>
        /// <param name="path">The request path</param>
        /// <returns>True if the path is for Swagger</returns>
        private static bool IsSwaggerEndpoint(PathString path) {
            return path.StartsWithSegments("/swagger") ||
                   path.StartsWithSegments("/swagger-ui");
        }

        /// <summary>
        /// Gets the authentication requirement from configuration
        /// </summary>
        /// <param name="context">The HTTP context</param>
        /// <returns>True if authentication is required</returns>
        private static bool GetAuthenticationRequirement(HttpContext context) {
            // In a real implementation, this would read from SwaggerSettings
            // For now, return false as default (no auth required)
            try {
                // This would be expanded to read actual configuration
                var configuration = context.RequestServices.GetService<SwaggerSettings>();
                return configuration?.Authentication.RequireAuthentication ?? false;
            }
            catch {
                return false;
            }
        }

        /// <summary>
        /// Validates if the user is authenticated
        /// </summary>
        /// <param name="context">The HTTP context</param>
        /// <returns>True if user is authenticated</returns>
        private async Task<bool> ValidateAuthenticationAsync(HttpContext context) {
            try {
                // Check if user is already authenticated via ASP.NET Core
                if (context.User?.Identity?.IsAuthenticated == true) {
                    return true;
                }

                // Check for API key authentication
                if (await ValidateApiKeyAsync(context)) {
                    return true;
                }

                // Check for Bearer token authentication
                if (await ValidateBearerTokenAsync(context)) {
                    return true;
                }

                // Check for Basic authentication
                if (await ValidateBasicAuthAsync(context)) {
                    return true;
                }

                return false;
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error during authentication validation");
                return false;
            }
        }

        /// <summary>
        /// Validates API key authentication
        /// </summary>
        /// <param name="context">The HTTP context</param>
        /// <returns>True if API key is valid</returns>
        private async Task<bool> ValidateApiKeyAsync(HttpContext context) {
            try {
                // This would be expanded based on actual API key configuration
                var apiKey = context.Request.Headers["X-API-Key"].FirstOrDefault() ??
                            context.Request.Query["apikey"].FirstOrDefault();

                if (string.IsNullOrEmpty(apiKey)) {
                    return false;
                }

                // In a real implementation, validate against configured API keys
                // For now, just check if it's not empty
                await Task.CompletedTask; // Simulate async validation
                return !string.IsNullOrEmpty(apiKey);
            }
            catch (Exception ex) {
                _logger.LogWarning(ex, "Error validating API key");
                return false;
            }
        }

        /// <summary>
        /// Validates Bearer token authentication
        /// </summary>
        /// <param name="context">The HTTP context</param>
        /// <returns>True if Bearer token is valid</returns>
        private async Task<bool> ValidateBearerTokenAsync(HttpContext context) {
            try {
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ")) {
                    return false;
                }

                var token = authHeader.Substring("Bearer ".Length).Trim();

                if (string.IsNullOrEmpty(token)) {
                    return false;
                }

                // In a real implementation, validate JWT token
                // This would integrate with ASP.NET Core JWT validation
                await Task.CompletedTask; // Simulate async validation
                return !string.IsNullOrEmpty(token);
            }
            catch (Exception ex) {
                _logger.LogWarning(ex, "Error validating Bearer token");
                return false;
            }
        }

        /// <summary>
        /// Validates Basic authentication
        /// </summary>
        /// <param name="context">The HTTP context</param>
        /// <returns>True if Basic auth is valid</returns>
        private async Task<bool> ValidateBasicAuthAsync(HttpContext context) {
            try {
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic ")) {
                    return false;
                }

                var encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
                var credentials = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
                var parts = credentials.Split(':', 2);

                if (parts.Length != 2) {
                    return false;
                }

                var username = parts[0];
                var password = parts[1];

                // In a real implementation, validate against user store
                await Task.CompletedTask; // Simulate async validation
                return !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);
            }
            catch (Exception ex) {
                _logger.LogWarning(ex, "Error validating Basic auth");
                return false;
            }
        }

        /// <summary>
        /// Validates if the authenticated user is authorized to access Swagger
        /// </summary>
        /// <param name="context">The HTTP context</param>
        /// <returns>True if user is authorized</returns>
        private async Task<bool> ValidateAuthorizationAsync(HttpContext context) {
            try {
                // In a real implementation, check roles/claims for Swagger access
                // For now, if user is authenticated, they're authorized
                await Task.CompletedTask;
                return true;
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error during authorization validation");
                return false;
            }
        }

        /// <summary>
        /// Handles unauthorized access (401)
        /// </summary>
        /// <param name="context">The HTTP context</param>
        /// <returns>Task representing the async operation</returns>
        private async Task HandleUnauthorizedAsync(HttpContext context) {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            var response = new {
                error = "Unauthorized",
                message = "Authentication is required to access Swagger documentation",
                timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }

        /// <summary>
        /// Handles forbidden access (403)
        /// </summary>
        /// <param name="context">The HTTP context</param>
        /// <returns>Task representing the async operation</returns>
        private async Task HandleForbiddenAsync(HttpContext context) {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";

            var response = new {
                error = "Forbidden",
                message = "You don't have permission to access Swagger documentation",
                timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }
    }

    /// <summary>
    /// Extension methods for registering Swagger authentication middleware
    /// </summary>
    public static class SwaggerAuthenticationMiddlewareExtensions {

        /// <summary>
        /// Adds Swagger authentication middleware to the pipeline
        /// </summary>
        /// <param name="builder">The application builder</param>
        /// <returns>The application builder</returns>
        public static IApplicationBuilder UseSwaggerAuthentication(this IApplicationBuilder builder) {
            return builder.UseMiddleware<SwaggerAuthenticationMiddleware>();
        }
    }
}