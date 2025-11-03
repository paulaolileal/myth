namespace Myth.Extensions.Swagger.Settings {

	/// <summary>
	/// Configuration for tree view hierarchical endpoint organization
	/// </summary>
	internal class TreeViewSettings {

		/// <summary>
		/// Enable hierarchical grouping based on tags
		/// </summary>
		public bool EnableHierarchy { get; set; } = true;

		/// <summary>
		/// Separator character for tag hierarchy (default: '/')
		/// </summary>
		public string TagSeparator { get; set; } = "/";

		/// <summary>
		/// Enable collapsible tree nodes
		/// </summary>
		public bool EnableCollapse { get; set; } = true;

		/// <summary>
		/// Default expand state for tree nodes
		/// </summary>
		public bool ExpandByDefault { get; set; } = false;

		/// <summary>
		/// Maximum nesting depth allowed
		/// </summary>
		public int MaxDepth { get; set; } = 5;

		/// <summary>
		/// Show endpoint count in tree nodes
		/// </summary>
		public bool ShowEndpointCount { get; set; } = true;
	}
}