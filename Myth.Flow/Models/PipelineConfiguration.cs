using System.Diagnostics;

namespace Myth.Models {

	/// <summary>
	/// Configuration options for Myth.Flow pipelines.
	/// Controls telemetry, logging, retry, exception filtering, and activity source settings.
	/// </summary>
	public sealed class PipelineConfiguration {

		/// <summary>
		/// Gets or sets a value indicating whether telemetry is enabled for pipeline execution.
		/// </summary>
		public bool EnableTelemetry { get; set; } = true;

		/// <summary>
		/// Gets or sets a value indicating whether logging is enabled for pipeline execution.
		/// </summary>
		public bool EnableLogging { get; set; } = true;

		/// <summary>
		/// Gets or sets the default number of retry attempts for pipeline steps.
		/// </summary>
		public int DefaultRetryAttempts { get; set; } = 0;

		/// <summary>
		/// Gets or sets the default backoff in milliseconds between retry attempts for pipeline steps.
		/// </summary>
		public int DefaultBackoffMs { get; set; } = 100;

		/// <summary>
		/// Gets or sets the <see cref="ActivitySource"/> used for telemetry instrumentation.
		/// </summary>
		public ActivitySource? ActivitySource { get; set; }

		/// <summary>
		/// Gets or sets the collection of exception types that should be propagated without being handled by the pipeline.
		/// By default, all exceptions are handled internally and returned as failure results.
		/// Exception types specified here will be thrown and not caught by the pipeline execution.
		/// </summary>
		public HashSet<Type> ExceptionTypesToPropagate { get; set; } = new HashSet<Type>( );
	}
}