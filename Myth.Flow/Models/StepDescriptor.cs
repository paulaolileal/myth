using System;
using System.Threading;
using System.Threading.Tasks;

namespace Myth.Models {

	/// <summary>
	/// Describes a step in the pipeline, including its type, handler, name, retry attempts, and backoff settings.
	/// </summary>
	internal readonly struct StepDescriptor<TContext> {

		/// <summary>
		/// Gets the type of the step (see <see cref="StepType"/>).
		/// </summary>
		public StepType Type { get; }

		/// <summary>
		/// Gets the handler function that executes the step logic asynchronously.
		/// </summary>
		public Func<TContext, CancellationToken, Task<TContext>> Handler { get; }

		/// <summary>
		/// Gets the name of the step, used for diagnostics and telemetry.
		/// </summary>
		public string? Name { get; }

		/// <summary>
		/// Gets the number of retry attempts for this step.
		/// </summary>
		public int RetryAttempts { get; }

		/// <summary>
		/// Gets the backoff in milliseconds between retry attempts for this step.
		/// </summary>
		public int BackoffMs { get; }

		/// <summary>
		/// Initializes a new instance of <see cref="StepDescriptor{TContext}"/> with the specified parameters.
		/// </summary>
		/// <param name="type">The type of the step (<see cref="StepType"/>).</param>
		/// <param name="handler">The handler function for the step.</param>
		/// <param name="name">Optional name for the step.</param>
		/// <param name="retryAttempts">Number of retry attempts.</param>
		/// <param name="backoffMs">Backoff in milliseconds between retries.</param>
		public StepDescriptor(
			StepType type,
			Func<TContext, CancellationToken, Task<TContext>> handler,
			string? name = null,
			int retryAttempts = 0,
			int backoffMs = 100 ) {
			Type = type;
			Handler = handler;
			Name = name;
			RetryAttempts = retryAttempts;
			BackoffMs = backoffMs;
		}
	}
}