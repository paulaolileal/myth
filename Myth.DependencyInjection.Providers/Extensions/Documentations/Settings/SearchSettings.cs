namespace Myth.Extensions.Swagger.Settings {

	/// <summary>
	/// Configuration for real-time endpoint search functionality
	/// </summary>
	internal class SearchSettings {

		/// <summary>
		/// Enable real-time search as user types
		/// </summary>
		public bool EnableRealTime { get; set; } = true;

		/// <summary>
		/// Fields to search in
		/// </summary>
		public SearchFields SearchFields { get; set; } = SearchFields.All;

		/// <summary>
		/// Minimum characters before search triggers
		/// </summary>
		public int MinSearchLength { get; set; } = 2;

		/// <summary>
		/// Debounce delay in milliseconds for real-time search
		/// </summary>
		public int DebounceMs { get; set; } = 300;

		/// <summary>
		/// Enable search result highlighting
		/// </summary>
		public bool EnableHighlighting { get; set; } = true;

		/// <summary>
		/// Enable search suggestions
		/// </summary>
		public bool EnableSuggestions { get; set; } = true;

		/// <summary>
		/// Case sensitive search
		/// </summary>
		public bool CaseSensitive { get; set; } = false;
	}

	/// <summary>
	/// Flags for search fields
	/// </summary>
	[Flags]
	public enum SearchFields {
		None = 0,
		Name = 1,
		Method = 2,
		Description = 4,
		Tags = 8,
		Path = 16,
		Parameters = 32,
		All = Name | Method | Description | Tags | Path | Parameters
	}
}