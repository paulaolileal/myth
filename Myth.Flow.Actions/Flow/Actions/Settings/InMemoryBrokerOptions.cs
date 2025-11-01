namespace Myth.Flow.Actions.Settings {

	/// <summary>
	/// Configuration options for InMemoryBroker
	/// </summary>
	public sealed class InMemoryBrokerOptions {

		/// <summary>
		/// Channel capacity for buffering messages
		/// </summary>
		public int ChannelCapacity { get; set; } = 10000;

		/// <summary>
		/// Number of worker tasks for parallel processing
		/// </summary>
		public int WorkerCount { get; set; } = Environment.ProcessorCount;

		/// <summary>
		/// Maximum number of concurrent handler executions
		/// </summary>
		public int MaxConcurrentHandlers { get; set; } = 100;

		/// <summary>
		/// Maximum retry attempts for failed messages
		/// </summary>
		public int MaxRetryAttempts { get; set; } = 3;

		/// <summary>
		/// Base retry backoff in milliseconds
		/// </summary>
		public int RetryBackoffMs { get; set; } = 1000;

		/// <summary>
		/// Maximum backoff delay in milliseconds
		/// </summary>
		public int MaxBackoffMs { get; set; } = 30000;

		/// <summary>
		/// Use exponential backoff for retries
		/// </summary>
		public bool ExponentialBackoff { get; set; } = true;

		/// <summary>
		/// Use dead letter queue for failed messages
		/// </summary>
		public bool UseDeadLetterQueue { get; set; } = true;
	}
}