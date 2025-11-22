namespace Myth.Models;

/// <summary>
/// Describes a step in the pipeline, including its type, handler, name, retry attempts, and backoff settings.
/// </summary>
/// <remarks>
/// Initializes a new instance of <see cref="StepDescriptor{TContext}"/> with the specified parameters.
/// </remarks>
/// <param name="type">The type of the step (<see cref="StepType"/>).</param>
/// <param name="handler">The handler function for the step.</param>
/// <param name="name">Optional name for the step.</param>
/// <param name="retryAttempts">Number of retry attempts.</param>
/// <param name="backoffMs">Backoff in milliseconds between retries.</param>
internal readonly struct StepDescriptor<TContext>(
	StepType type,
	Func<TContext, CancellationToken, Task<TContext>> handler,
	string? name = null,
	int retryAttempts = 0,
	int backoffMs = 100 ) {

	/// <summary>
	/// Gets the type of the step (see <see cref="StepType"/>).
	/// </summary>
	public StepType Type { get; } = type;

	/// <summary>
	/// Gets the handler function that executes the step logic asynchronously.
	/// </summary>
	public Func<TContext, CancellationToken, Task<TContext>> Handler { get; } = handler;

	/// <summary>
	/// Gets the name of the step, used for diagnostics and telemetry.
	/// </summary>
	public string? Name { get; } = name;

	/// <summary>
	/// Gets the number of retry attempts for this step.
	/// </summary>
	public int RetryAttempts { get; } = retryAttempts;

	/// <summary>
	/// Gets the backoff in milliseconds between retry attempts for this step.
	/// </summary>
	public int BackoffMs { get; } = backoffMs;
}
