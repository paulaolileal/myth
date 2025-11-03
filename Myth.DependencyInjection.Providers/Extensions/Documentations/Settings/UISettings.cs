namespace Myth.Extensions.Swagger.Settings {

	/// <summary>
	/// Configuration for UI/UX enhancements
	/// </summary>
	internal class UISettings {

		/// <summary>
		/// Enable keyboard shortcuts (Ctrl+Enter, etc.)
		/// </summary>
		public bool EnableKeyboardShortcuts { get; set; } = true;

		/// <summary>
		/// Enable direct execution without 'Try it out' button
		/// </summary>
		public bool EnableDirectExecution { get; set; } = true;

		/// <summary>
		/// Enable JSON beautification before sending requests
		/// </summary>
		public bool EnableJsonBeautify { get; set; } = true;

		/// <summary>
		/// Hide models by default with expand button
		/// </summary>
		public bool EnableModelCollapse { get; set; } = true;

		/// <summary>
		/// Keyboard shortcuts configuration
		/// </summary>
		public KeyboardShortcutsSettings KeyboardShortcuts { get; set; } = new( );

		/// <summary>
		/// Execution button configuration
		/// </summary>
		public ExecutionSettings Execution { get; set; } = new( );

		/// <summary>
		/// JSON formatting configuration
		/// </summary>
		public JsonFormattingSettings JsonFormatting { get; set; } = new( );

		/// <summary>
		/// Model display configuration
		/// </summary>
		public ModelDisplaySettings ModelDisplay { get; set; } = new( );

		/// <summary>
		/// Layout configuration
		/// </summary>
		public LayoutSettings Layout { get; set; } = new( );
	}

	/// <summary>
	/// Keyboard shortcuts configuration
	/// </summary>
	internal class KeyboardShortcutsSettings {

		/// <summary>
		/// Shortcut to execute request (default: Ctrl+Enter)
		/// </summary>
		public string ExecuteRequest { get; set; } = "Ctrl+Enter";

		/// <summary>
		/// Shortcut to clear form (default: Ctrl+Delete)
		/// </summary>
		public string ClearForm { get; set; } = "Ctrl+Delete";

		/// <summary>
		/// Shortcut to focus search (default: Ctrl+F)
		/// </summary>
		public string FocusSearch { get; set; } = "Ctrl+F";

		/// <summary>
		/// Shortcut to toggle theme (default: Ctrl+Shift+T)
		/// </summary>
		public string ToggleTheme { get; set; } = "Ctrl+Shift+T";

		/// <summary>
		/// Shortcut to beautify JSON (default: Ctrl+Shift+F)
		/// </summary>
		public string BeautifyJson { get; set; } = "Ctrl+Shift+F";

		/// <summary>
		/// Enable help tooltip for shortcuts
		/// </summary>
		public bool ShowHelp { get; set; } = true;
	}

	/// <summary>
	/// Execution button configuration
	/// </summary>
	internal class ExecutionSettings {

		/// <summary>
		/// Validate required fields before execution
		/// </summary>
		public bool ValidateRequiredFields { get; set; } = true;

		/// <summary>
		/// Show confirmation dialog for destructive operations (DELETE, etc.)
		/// </summary>
		public bool ConfirmDestructiveOperations { get; set; } = true;

		/// <summary>
		/// Auto-set Content-Type based on request body
		/// </summary>
		public bool AutoSetContentType { get; set; } = true;

		/// <summary>
		/// Enable button only when form is valid
		/// </summary>
		public bool EnableOnlyWhenValid { get; set; } = true;

		/// <summary>
		/// Button text for different HTTP methods
		/// </summary>
		public Dictionary<string, string> ButtonTexts { get; set; } = new( ) {
			{ "GET", "🔍 Fetch" },
			{ "POST", "📤 Create" },
			{ "PUT", "📝 Update" },
			{ "PATCH", "🔧 Modify" },
			{ "DELETE", "🗑️ Delete" }
		};
	}

	/// <summary>
	/// JSON formatting configuration
	/// </summary>
	internal class JsonFormattingSettings {

		/// <summary>
		/// Auto-format JSON on input
		/// </summary>
		public bool AutoFormat { get; set; } = true;

		/// <summary>
		/// Number of spaces for indentation
		/// </summary>
		public int IndentSpaces { get; set; } = 2;

		/// <summary>
		/// Enable syntax highlighting for JSON
		/// </summary>
		public bool EnableSyntaxHighlighting { get; set; } = true;

		/// <summary>
		/// Show beautify button
		/// </summary>
		public bool ShowBeautifyButton { get; set; } = true;

		/// <summary>
		/// Validate JSON syntax
		/// </summary>
		public bool ValidateSyntax { get; set; } = true;
	}

	/// <summary>
	/// Model display configuration
	/// </summary>
	internal class ModelDisplaySettings {

		/// <summary>
		/// Collapse models by default
		/// </summary>
		public bool CollapseByDefault { get; set; } = true;

		/// <summary>
		/// Show recursive model references
		/// </summary>
		public bool ShowRecursiveReferences { get; set; } = true;

		/// <summary>
		/// Maximum nesting depth for recursive models
		/// </summary>
		public int MaxRecursionDepth { get; set; } = 3;

		/// <summary>
		/// Show property descriptions
		/// </summary>
		public bool ShowPropertyDescriptions { get; set; } = true;

		/// <summary>
		/// Show property examples
		/// </summary>
		public bool ShowPropertyExamples { get; set; } = true;

		/// <summary>
		/// Compact model display
		/// </summary>
		public bool CompactDisplay { get; set; } = false;
	}

	/// <summary>
	/// Layout configuration
	/// </summary>
	internal class LayoutSettings {

		/// <summary>
		/// Compact layout with reduced spacing
		/// </summary>
		public bool CompactMode { get; set; } = false;

		/// <summary>
		/// Hide Swagger logo
		/// </summary>
		public bool HideLogo { get; set; } = false;

		/// <summary>
		/// Hide "Try it out" text
		/// </summary>
		public bool HideTryItOutText { get; set; } = true;

		/// <summary>
		/// Sticky navigation header
		/// </summary>
		public bool StickyHeader { get; set; } = true;

		/// <summary>
		/// Enable responsive design for mobile
		/// </summary>
		public bool EnableResponsive { get; set; } = true;

		/// <summary>
		/// Custom CSS classes to add to root element
		/// </summary>
		public List<string> CustomCssClasses { get; set; } = new( );
	}
}