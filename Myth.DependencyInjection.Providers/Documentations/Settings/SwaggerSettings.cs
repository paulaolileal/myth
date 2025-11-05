using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Myth.Documentations.Settings {

    public partial class SwaggerSettings() {

        /// <summary>
        /// The title of API
        /// </summary>
        internal string Title { get; set; } = null!;

        public SwaggerSettings UseTitle(string title) {
            Title = title;
            return this;
        }

        /// <summary>
        /// The description of API. The purpose of API
        /// </summary>
        internal string Description { get; set; } = null!;

        public SwaggerSettings UseDescription(string description) {
            Description = description;
            return this;
        }

        /// <summary>
        /// The name of authors and contact point
        /// </summary>
        internal string ContactName { get; set; } = null!;

        /// <summary>
        /// The email of authors and contact point
        /// </summary>
        internal string ContactEmail { get; set; } = null!;

        /// <summary>
        /// The url of application and docs
        /// </summary>
        internal string ContactUrl { get; set; } = null!;

        public SwaggerSettings UseContact(string name, string email, string url) {
            ContactName = name;
            ContactEmail = email;
            ContactUrl = url;
            return this;
        }

        internal AuthorizationType Type { get; set; } = AuthorizationType.None;

        internal SwaggerSettings UseAuthorization(AuthorizationType type) {
            Type = type;
            return this;
        }

        /// <summary>
        /// Adds basic authorization to the Swagger documentation.
        /// </summary>
        /// <returns>This settings updated</returns>
        public SwaggerSettings UseBasicAuthorization() => UseAuthorization(AuthorizationType.Basic);

        /// <summary>
        /// Adds bearer token authorization to the Swagger documentation.
        /// </summary>
        /// <returns>This settings updated</returns>
        public SwaggerSettings UseBearerAuthorization() => UseAuthorization(AuthorizationType.Bearer);

        /// <summary>
        /// Adds API key authorization to the Swagger documentation.
        /// </summary>
        /// <returns>This settings updated</returns>
        public SwaggerSettings UseApiKeyAuthorization() => UseAuthorization(AuthorizationType.ApiKey);

        // === ADVANCED UI FEATURES ===

        /// <summary>
        /// Tree view configuration for hierarchical endpoint organization
        /// </summary>
        internal TreeViewSettings TreeView { get; set; } = new();

        /// <summary>
        /// Configures tree view for hierarchical endpoint organization using tags
        /// </summary>
        /// <param name="enableHierarchy">Enable hierarchical grouping based on tags</param>
        /// <param name="tagSeparator">Separator character for tag hierarchy (default: '/')</param>
        /// <returns>This settings updated</returns>
        public SwaggerSettings UseTreeView(bool enableHierarchy = true, string tagSeparator = "/") {
            TreeView.EnableHierarchy = enableHierarchy;
            TreeView.TagSeparator = tagSeparator;
            return this;
        }

        /// <summary>
        /// Search configuration for real-time endpoint filtering
        /// </summary>
        internal SearchSettings Search { get; set; } = new();

        /// <summary>
        /// Configures real-time search functionality
        /// </summary>
        /// <param name="enableRealTime">Enable real-time search as user types</param>
        /// <param name="searchFields">Fields to search in (name, method, description, etc.)</param>
        /// <returns>This settings updated</returns>
        public SwaggerSettings UseSearch(bool enableRealTime = true, SearchFields searchFields = SearchFields.All) {
            Search.EnableRealTime = enableRealTime;
            Search.SearchFields = searchFields;
            return this;
        }

        /// <summary>
        /// Theme configuration for UI appearance
        /// </summary>
        internal ThemeSettings Theme { get; set; } = new();

        /// <summary>
        /// Configures UI theme settings
        /// </summary>
        /// <param name="defaultTheme">Default theme (Light, Dark, Auto)</param>
        /// <param name="allowUserToggle">Allow users to switch themes</param>
        /// <returns>This settings updated</returns>
        public SwaggerSettings UseTheme(SwaggerTheme defaultTheme = SwaggerTheme.Auto, bool allowUserToggle = true) {
            Theme.DefaultTheme = defaultTheme;
            Theme.AllowUserToggle = allowUserToggle;
            return this;
        }

        /// <summary>
        /// Cache configuration for persistent storage
        /// </summary>
        internal CacheSettings Cache { get; set; } = new();

        /// <summary>
        /// Configures persistent cache for parameters and request bodies
        /// </summary>
        /// <param name="enablePersistence">Enable cache persistence across browser sessions</param>
        /// <param name="expirationMinutes">Cache expiration time in minutes (0 = no expiration)</param>
        /// <param name="enableHistory">Enable request history per endpoint</param>
        /// <returns>This settings updated</returns>
        public SwaggerSettings UseCache(bool enablePersistence = true, int expirationMinutes = 60, bool enableHistory = true) {
            Cache.EnablePersistence = enablePersistence;
            Cache.ExpirationMinutes = expirationMinutes;
            Cache.EnableHistory = enableHistory;
            return this;
        }

        /// <summary>
        /// Authentication integration configuration
        /// </summary>
        internal AuthenticationSettings Authentication { get; set; } = new();

        /// <summary>
        /// Configures advanced authentication integration
        /// </summary>
        /// <param name="enableDropdown">Enable authentication method dropdown</param>
        /// <param name="validateTokens">Validate tokens against ASP.NET Core authentication</param>
        /// <param name="requireAuth">Require authentication to access Swagger</param>
        /// <returns>This settings updated</returns>
        public SwaggerSettings UseAuthentication(bool enableDropdown = true, bool validateTokens = false, bool requireAuth = false) {
            Authentication.EnableDropdown = enableDropdown;
            Authentication.ValidateTokens = validateTokens;
            Authentication.RequireAuthentication = requireAuth;
            return this;
        }

        /// <summary>
        /// UI/UX enhancement configuration
        /// </summary>
        internal UISettings UI { get; set; } = new();

        /// <summary>
        /// Configures UI/UX enhancements
        /// </summary>
        /// <param name="enableKeyboardShortcuts">Enable keyboard shortcuts (Ctrl+Enter, etc.)</param>
        /// <param name="enableDirectExecution">Enable direct execution without 'Try it out'</param>
        /// <param name="enableJsonBeautify">Enable JSON beautification before sending requests</param>
        /// <param name="enableModelCollapse">Hide models by default with expand button</param>
        /// <returns>This settings updated</returns>
        public SwaggerSettings UseUI(bool enableKeyboardShortcuts = true, bool enableDirectExecution = true, bool enableJsonBeautify = true, bool enableModelCollapse = true) {
            UI.EnableKeyboardShortcuts = enableKeyboardShortcuts;
            UI.EnableDirectExecution = enableDirectExecution;
            UI.EnableJsonBeautify = enableJsonBeautify;
            UI.EnableModelCollapse = enableModelCollapse;
            return this;
        }

        /// <summary>
        /// Performance and feedback configuration
        /// </summary>
        internal PerformanceSettings Performance { get; set; } = new();

        /// <summary>
        /// Configures performance monitoring and visual feedback
        /// </summary>
        /// <param name="enableTiming">Show request timing in results</param>
        /// <param name="enableStatusColors">Color-code HTTP status codes</param>
        /// <param name="enableProgressIndicators">Show loading indicators during requests</param>
        /// <returns>This settings updated</returns>
        public SwaggerSettings UsePerformance(bool enableTiming = true, bool enableStatusColors = true, bool enableProgressIndicators = true) {
            Performance.EnableTiming = enableTiming;
            Performance.EnableStatusColors = enableStatusColors;
            Performance.EnableProgressIndicators = enableProgressIndicators;
            return this;
        }

        /// <summary>
        /// Swagger UI access authentication configuration
        /// </summary>
        internal SwaggerAuthenticationSettings SwaggerAuthentication { get; set; } = new();

        /// <summary>
        /// Configures authentication requirements for accessing Swagger UI documentation
        /// </summary>
        /// <param name="requireAuthentication">Require authentication to access Swagger UI</param>
        /// <param name="validateTokens">Validate tokens against ASP.NET Core authentication system</param>
        /// <param name="allowedSchemes">Authentication schemes allowed for Swagger access</param>
        /// <returns>This settings updated</returns>
        /// <remarks>
        /// This method integrates authentication middleware directly into the Swagger UI pipeline,
        /// eliminating the need for separate middleware registration. Commonly used in production
        /// environments to protect API documentation.
        /// </remarks>
        public SwaggerSettings RequireAuthentication(bool requireAuthentication = true, bool validateTokens = true, params AuthorizationType[] allowedSchemes) {
            SwaggerAuthentication.RequireAuthentication = requireAuthentication;
            SwaggerAuthentication.ValidateTokens = validateTokens;

            if (allowedSchemes?.Length > 0) {
                SwaggerAuthentication.AllowedSchemes = allowedSchemes.ToList();
            }

            return this;
        }

        /// <summary>
        /// Configures authentication requirements based on the current environment
        /// </summary>
        /// <param name="environmentCondition">Function to determine if authentication should be required based on environment</param>
        /// <param name="validateTokens">Validate tokens against ASP.NET Core authentication system</param>
        /// <param name="allowedSchemes">Authentication schemes allowed for Swagger access</param>
        /// <returns>This settings updated</returns>
        /// <remarks>
        /// This method allows environment-specific authentication requirements, such as requiring
        /// authentication only in production while keeping development environments open.
        /// </remarks>
        public SwaggerSettings RequireAuthenticationIf(Func<IWebHostEnvironment, bool> environmentCondition, bool validateTokens = true, params AuthorizationType[] allowedSchemes) {
            SwaggerAuthentication.EnvironmentBasedRequirement = environmentCondition;
            SwaggerAuthentication.ValidateTokens = validateTokens;

            if (allowedSchemes?.Length > 0) {
                SwaggerAuthentication.AllowedSchemes = allowedSchemes.ToList();
            }

            return this;
        }

        /// <summary>
        /// Configures custom authentication validation logic for Swagger UI access
        /// </summary>
        /// <param name="customValidator">Custom function to validate authentication</param>
        /// <param name="requireAuthentication">Enable the authentication requirement</param>
        /// <returns>This settings updated</returns>
        /// <remarks>
        /// This method allows completely custom authentication logic, useful for integrating
        /// with external authentication systems or implementing custom authorization rules.
        /// </remarks>
        public SwaggerSettings RequireCustomAuthentication(Func<HttpContext, Task<bool>> customValidator, bool requireAuthentication = true) {
            SwaggerAuthentication.CustomAuthenticationValidator = customValidator;
            SwaggerAuthentication.RequireAuthentication = requireAuthentication;
            return this;
        }

        /// <summary>
        /// Configures paths that bypass authentication requirements
        /// </summary>
        /// <param name="bypassPaths">Paths that don't require authentication</param>
        /// <returns>This settings updated</returns>
        /// <remarks>
        /// This method allows certain paths to bypass authentication, useful for health checks
        /// or public documentation sections that should remain accessible.
        /// </remarks>
        public SwaggerSettings WithAuthenticationBypass(params string[] bypassPaths) {
            SwaggerAuthentication.BypassPaths = bypassPaths?.ToList() ?? new List<string>();
            return this;
        }

        /// <summary>
        /// Configures all advanced features with sensible defaults
        /// </summary>
        /// <returns>This settings updated</returns>
        public SwaggerSettings UseAdvancedFeatures() {
            return UseTreeView()
                .UseSearch()
                .UseTheme()
                .UseCache()
                .UseAuthentication()
                .UseUI()
                .UsePerformance();
        }
    }
}