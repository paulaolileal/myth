namespace Myth.Documentations.Settings {

    /// <summary>
    /// Configuration for performance monitoring and visual feedback
    /// </summary>
    internal class PerformanceSettings {

        /// <summary>
        /// Show request timing in results
        /// </summary>
        public bool EnableTiming { get; set; } = true;

        /// <summary>
        /// Color-code HTTP status codes
        /// </summary>
        public bool EnableStatusColors { get; set; } = true;

        /// <summary>
        /// Show loading indicators during requests
        /// </summary>
        public bool EnableProgressIndicators { get; set; } = true;

        /// <summary>
        /// Request timing configuration
        /// </summary>
        public TimingSettings Timing { get; set; } = new();

        /// <summary>
        /// Status code color configuration
        /// </summary>
        public StatusColorSettings StatusColors { get; set; } = new();

        /// <summary>
        /// Progress indicator configuration
        /// </summary>
        public ProgressIndicatorSettings ProgressIndicators { get; set; } = new();

        /// <summary>
        /// Response feedback configuration
        /// </summary>
        public ResponseFeedbackSettings ResponseFeedback { get; set; } = new();
    }

    /// <summary>
    /// Request timing configuration
    /// </summary>
    internal class TimingSettings {

        /// <summary>
        /// Show detailed timing breakdown
        /// </summary>
        public bool ShowDetailedTiming { get; set; } = true;

        /// <summary>
        /// Show timing in milliseconds
        /// </summary>
        public bool ShowInMilliseconds { get; set; } = true;

        /// <summary>
        /// Highlight slow requests (above threshold)
        /// </summary>
        public bool HighlightSlowRequests { get; set; } = true;

        /// <summary>
        /// Slow request threshold in milliseconds
        /// </summary>
        public int SlowRequestThresholdMs { get; set; } = 2000;

        /// <summary>
        /// Show timing in response header
        /// </summary>
        public bool ShowInHeader { get; set; } = true;

        /// <summary>
        /// Keep timing history
        /// </summary>
        public bool KeepHistory { get; set; } = true;

        /// <summary>
        /// Maximum history entries per endpoint
        /// </summary>
        public int MaxHistoryEntries { get; set; } = 10;
    }

    /// <summary>
    /// Status code color configuration
    /// </summary>
    internal class StatusColorSettings {

        /// <summary>
        /// Colors for different status code ranges
        /// </summary>
        public Dictionary<string, string> StatusRangeColors { get; set; } = new() {
            { "1xx", "#6c757d" }, // Info - Gray
			{ "2xx", "#28a745" }, // Success - Green
			{ "3xx", "#ffc107" }, // Redirect - Yellow
			{ "4xx", "#fd7e14" }, // Client Error - Orange
			{ "5xx", "#dc3545" }  // Server Error - Red
		};

        /// <summary>
        /// Apply colors to status code text
        /// </summary>
        public bool ColorStatusText { get; set; } = true;

        /// <summary>
        /// Apply colors to response container background
        /// </summary>
        public bool ColorBackground { get; set; } = false;

        /// <summary>
        /// Apply colors to response border
        /// </summary>
        public bool ColorBorder { get; set; } = true;

        /// <summary>
        /// Use subtle colors
        /// </summary>
        public bool UseSubtleColors { get; set; } = true;
    }

    /// <summary>
    /// Progress indicator configuration
    /// </summary>
    internal class ProgressIndicatorSettings {

        /// <summary>
        /// Type of progress indicator
        /// </summary>
        public ProgressIndicatorType Type { get; set; } = ProgressIndicatorType.Spinner;

        /// <summary>
        /// Show progress on button
        /// </summary>
        public bool ShowOnButton { get; set; } = true;

        /// <summary>
        /// Show progress in response area
        /// </summary>
        public bool ShowInResponseArea { get; set; } = true;

        /// <summary>
        /// Disable form during request
        /// </summary>
        public bool DisableFormDuringRequest { get; set; } = true;

        /// <summary>
        /// Custom loading messages
        /// </summary>
        public List<string> LoadingMessages { get; set; } = new() {
            "Sending request...",
            "Processing...",
            "Waiting for response..."
        };

        /// <summary>
        /// Animation duration in milliseconds
        /// </summary>
        public int AnimationDurationMs { get; set; } = 300;
    }

    /// <summary>
    /// Response feedback configuration
    /// </summary>
    internal class ResponseFeedbackSettings {

        /// <summary>
        /// Show response size
        /// </summary>
        public bool ShowResponseSize { get; set; } = true;

        /// <summary>
        /// Show response headers count
        /// </summary>
        public bool ShowHeadersCount { get; set; } = true;

        /// <summary>
        /// Highlight JSON response
        /// </summary>
        public bool HighlightJsonResponse { get; set; } = true;

        /// <summary>
        /// Show copy response button
        /// </summary>
        public bool ShowCopyButton { get; set; } = true;

        /// <summary>
        /// Show download response button
        /// </summary>
        public bool ShowDownloadButton { get; set; } = true;

        /// <summary>
        /// Auto-expand successful responses
        /// </summary>
        public bool AutoExpandSuccess { get; set; } = true;

        /// <summary>
        /// Auto-expand error responses
        /// </summary>
        public bool AutoExpandErrors { get; set; } = true;

        /// <summary>
        /// Show success/error icons
        /// </summary>
        public bool ShowStatusIcons { get; set; } = true;

        /// <summary>
        /// Toast notification settings
        /// </summary>
        public ToastSettings Toast { get; set; } = new();
    }

    /// <summary>
    /// Toast notification settings
    /// </summary>
    internal class ToastSettings {

        /// <summary>
        /// Enable toast notifications
        /// </summary>
        public bool Enable { get; set; } = false;

        /// <summary>
        /// Show toast for successful requests
        /// </summary>
        public bool ShowForSuccess { get; set; } = false;

        /// <summary>
        /// Show toast for error requests
        /// </summary>
        public bool ShowForErrors { get; set; } = true;

        /// <summary>
        /// Toast duration in milliseconds
        /// </summary>
        public int DurationMs { get; set; } = 3000;

        /// <summary>
        /// Toast position
        /// </summary>
        public ToastPosition Position { get; set; } = ToastPosition.TopRight;
    }

    /// <summary>
    /// Progress indicator types
    /// </summary>
    internal enum ProgressIndicatorType {
        Spinner,
        ProgressBar,
        Dots,
        Pulse
    }

    /// <summary>
    /// Toast notification positions
    /// </summary>
    internal enum ToastPosition {
        TopLeft,
        TopCenter,
        TopRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }
}