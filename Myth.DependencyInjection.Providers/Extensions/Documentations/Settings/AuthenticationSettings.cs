using static Myth.Extensions.Swagger.Settings.SwaggerSettings;

namespace Myth.Extensions.Swagger.Settings {

	/// <summary>
	/// Configuration for advanced authentication integration
	/// </summary>
	internal class AuthenticationSettings {

		/// <summary>
		/// Enable authentication method dropdown in UI
		/// </summary>
		public bool EnableDropdown { get; set; } = true;

		/// <summary>
		/// Validate tokens against ASP.NET Core authentication
		/// </summary>
		public bool ValidateTokens { get; set; } = false;

		/// <summary>
		/// Require authentication to access Swagger
		/// </summary>
		public bool RequireAuthentication { get; set; } = false;

		/// <summary>
		/// Allowed authentication schemes
		/// </summary>
		public List<AuthorizationType> AllowedSchemes { get; set; } = new( ) {
			AuthorizationType.Bearer,
			AuthorizationType.Basic,
			AuthorizationType.ApiKey
		};

		/// <summary>
		/// Secure credential storage settings
		/// </summary>
		public CredentialStorageSettings CredentialStorage { get; set; } = new( );

		/// <summary>
		/// OAuth2 configuration if applicable
		/// </summary>
		public OAuth2Settings? OAuth2 { get; set; }

		/// <summary>
		/// API Key configuration
		/// </summary>
		public ApiKeySettings ApiKey { get; set; } = new( );

		/// <summary>
		/// Custom authorization headers
		/// </summary>
		public Dictionary<string, string> CustomHeaders { get; set; } = new( );

		/// <summary>
		/// Token refresh settings
		/// </summary>
		public TokenRefreshSettings TokenRefresh { get; set; } = new( );
	}

	/// <summary>
	/// Configuration for secure credential storage
	/// </summary>
	internal class CredentialStorageSettings {

		/// <summary>
		/// Encrypt stored credentials
		/// </summary>
		public bool EnableEncryption { get; set; } = true;

		/// <summary>
		/// Isolate credentials by domain
		/// </summary>
		public bool IsolateByDomain { get; set; } = true;

		/// <summary>
		/// Credential expiration time in minutes
		/// </summary>
		public int ExpirationMinutes { get; set; } = 480; // 8 hours

		/// <summary>
		/// Clear credentials on browser close
		/// </summary>
		public bool ClearOnClose { get; set; } = false;
	}

	/// <summary>
	/// OAuth2 configuration
	/// </summary>
	internal class OAuth2Settings {

		/// <summary>
		/// OAuth2 authorization URL
		/// </summary>
		public string AuthorizationUrl { get; set; } = null!;

		/// <summary>
		/// OAuth2 token URL
		/// </summary>
		public string TokenUrl { get; set; } = null!;

		/// <summary>
		/// OAuth2 scopes
		/// </summary>
		public Dictionary<string, string> Scopes { get; set; } = new( );

		/// <summary>
		/// Client ID
		/// </summary>
		public string ClientId { get; set; } = null!;

		/// <summary>
		/// PKCE support
		/// </summary>
		public bool UsePkce { get; set; } = true;
	}

	/// <summary>
	/// API Key configuration
	/// </summary>
	internal class ApiKeySettings {

		/// <summary>
		/// API Key header name
		/// </summary>
		public string HeaderName { get; set; } = "X-API-Key";

		/// <summary>
		/// API Key parameter location
		/// </summary>
		public ApiKeyLocation Location { get; set; } = ApiKeyLocation.Header;

		/// <summary>
		/// API Key parameter name (for query or header)
		/// </summary>
		public string ParameterName { get; set; } = "apikey";
	}

	/// <summary>
	/// Token refresh configuration
	/// </summary>
	internal class TokenRefreshSettings {

		/// <summary>
		/// Enable automatic token refresh
		/// </summary>
		public bool EnableAutoRefresh { get; set; } = false;

		/// <summary>
		/// Refresh token endpoint
		/// </summary>
		public string? RefreshEndpoint { get; set; }

		/// <summary>
		/// Minutes before expiration to refresh token
		/// </summary>
		public int RefreshBeforeExpirationMinutes { get; set; } = 5;
	}

	/// <summary>
	/// API Key location options
	/// </summary>
	internal enum ApiKeyLocation {
		Header,
		Query,
		Cookie
	}
}