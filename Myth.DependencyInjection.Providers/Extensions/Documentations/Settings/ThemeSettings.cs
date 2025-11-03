namespace Myth.Extensions.Swagger.Settings {

	/// <summary>
	/// Configuration for UI theme settings
	/// </summary>
	internal class ThemeSettings {

		/// <summary>
		/// Default theme on load
		/// </summary>
		public SwaggerTheme DefaultTheme { get; set; } = SwaggerTheme.Auto;

		/// <summary>
		/// Allow users to toggle between themes
		/// </summary>
		public bool AllowUserToggle { get; set; } = true;

		/// <summary>
		/// Persist theme preference in localStorage
		/// </summary>
		public bool PersistPreference { get; set; } = true;

		/// <summary>
		/// Enable smooth transitions between themes
		/// </summary>
		public bool EnableTransitions { get; set; } = true;

		/// <summary>
		/// Custom CSS variables for theme customization
		/// </summary>
		public Dictionary<string, string> CustomVariables { get; set; } = new( );
	}

	/// <summary>
	/// Available theme options
	/// </summary>
	public enum SwaggerTheme {

		/// <summary>
		/// Light theme
		/// </summary>
		Light,

		/// <summary>
		/// Dark theme
		/// </summary>
		Dark,

		/// <summary>
		/// Auto theme based on system preference
		/// </summary>
		Auto
	}
}