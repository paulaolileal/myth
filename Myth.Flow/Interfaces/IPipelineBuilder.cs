using Myth.Models;

namespace Myth.Interfaces;

/// <summary>
/// Main pipeline builder interface for constructing and executing pipelines.
/// Provides fluent methods for adding steps, transformations, taps, conditionals, telemetry, and retry logic.
/// </summary>
public interface IPipelineBuilder<TContext> {

	/// <summary>
	/// Adds a synchronous step to the pipeline.
	/// </summary>
	/// <param name="handler">Step handler function that takes the current context and returns a new context.</param>
	/// <param name="onSuccess">Optional callback on success.</param>
	/// <param name="onError">Optional error handler for this step.</param>
	/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
	IPipelineBuilder<TContext> Step(
		Func<TContext, TContext> handler,
		Action<TContext>? onSuccess = null,
		Action<Exception>? onError = null );

	/// <summary>
	/// Adds an asynchronous step to the pipeline.
	/// </summary>
	/// <param name="handler">Async step handler function that takes the current context and returns a new context.</param>
	/// <param name="onSuccess">Optional callback on success.</param>
	/// <param name="onError">Optional error handler for this step.</param>
	/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
	IPipelineBuilder<TContext> StepAsync(
		Func<TContext, Task<TContext>> handler,
		Action<TContext>? onSuccess = null,
		Action<Exception>? onError = null );

	/// <summary>
	/// Adds a synchronous step to the pipeline that returns a <see cref="Result{TContext}"/>.
	/// Throws <see cref="Exceptions.PipelineException"/> if the result is failure.
	/// </summary>
	/// <param name="handler">Step handler returning a <see cref="Result{TContext}"/>.</param>
	/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
	IPipelineBuilder<TContext> StepResult(
		Func<TContext, Result<TContext>> handler );

	/// <summary>
	/// Adds an asynchronous step to the pipeline that returns a <see cref="Result{TContext}"/>.
	/// Throws <see cref="Exceptions.PipelineException"/> if the result is failure.
	/// </summary>
	/// <param name="handler">Async step handler returning a <see cref="Result{TContext}"/>.</param>
	/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
	IPipelineBuilder<TContext> StepResultAsync(
		Func<TContext, Task<Result<TContext>>> handler );

	/// <summary>
	/// Transforms the pipeline context to a new type using the provided mapper function.
	/// All previous steps are executed before transformation.
	/// </summary>
	/// <typeparam name="TNewContext">Type of the new context.</typeparam>
	/// <param name="mapper">Function to map the old context to the new context.</param>
	/// <returns>A new <see cref="IPipelineBuilder{TNewContext}"/> instance.</returns>
	IPipelineBuilder<TNewContext> Transform<TNewContext>(
		Func<TContext, TNewContext> mapper );

	/// <summary>
	/// Asynchronously transforms the pipeline context to a new type using the provided async mapper function.
	/// All previous steps are executed before transformation.
	/// </summary>
	/// <typeparam name="TNewContext">Type of the new context.</typeparam>
	/// <param name="mapper">Async function to map the old context to the new context.</param>
	/// <returns>A new <see cref="IPipelineBuilder{TNewContext}"/> instance.</returns>
	IPipelineBuilder<TNewContext> TransformAsync<TNewContext>(
		Func<TContext, Task<TNewContext>> mapper );

	/// <summary>
	/// Adds a tap step to the pipeline that executes a side-effect action on the context.
	/// </summary>
	/// <param name="action">Action to execute on the context.</param>
	/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
	IPipelineBuilder<TContext> Tap( Action<TContext> action );

	/// <summary>
	/// Adds an asynchronous tap step to the pipeline that executes a side-effect async action on the context.
	/// </summary>
	/// <param name="action">Async action to execute on the context.</param>
	/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
	IPipelineBuilder<TContext> TapAsync( Func<TContext, Task> action );

	/// <summary>
	/// Adds a conditional step to the pipeline. Executes the configured pipeline if the predicate returns true.
	/// </summary>
	/// <param name="predicate">Predicate to determine if the conditional pipeline should run.</param>
	/// <param name="configurePipeline">Action to configure the conditional pipeline.</param>
	/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
	IPipelineBuilder<TContext> When(
		Func<TContext, bool> predicate,
		Action<IPipelineBuilder<TContext>> configurePipeline );

	/// <summary>
	/// Enables telemetry for the pipeline execution with the specified operation name.
	/// </summary>
	/// <param name="operationName">Telemetry operation name.</param>
	/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
	IPipelineBuilder<TContext> WithTelemetry( string operationName );

	/// <summary>
	/// Configures retry attempts and backoff for subsequent steps in the pipeline.
	/// </summary>
	/// <param name="maxAttempts">Maximum number of retry attempts.</param>
	/// <param name="backoffMs">Backoff in milliseconds between retries.</param>
	/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
	IPipelineBuilder<TContext> WithRetry( int maxAttempts, int backoffMs = 100 );

	/// <summary>
	/// Executes the pipeline asynchronously, running all configured steps in order.
	/// </summary>
	/// <param name="cancellationToken">Optional cancellation token.</param>
	/// <returns>A <see cref="Result{TContext}"/> representing the outcome of the pipeline execution.</returns>
	Task<Result<TContext>> ExecuteAsync( CancellationToken cancellationToken = default );

	/// <summary>
	/// Executes the pipeline synchronously, running all configured steps in order.
	/// </summary>
	/// <returns>A <see cref="Result{TContext}"/> representing the outcome of the pipeline execution.</returns>
	Result<TContext> Execute( );

	/// <summary>
	/// Adds an asynchronous step to the pipeline with cancellation token support.
	/// </summary>
	/// <param name="handler">Async step handler function that takes the current context and cancellation token, returns a new context.</param>
	/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
	IPipelineBuilder<TContext> StepAsync(
		Func<TContext, CancellationToken, Task<TContext>> handler );

	/// <summary>
	/// Adds an asynchronous step to the pipeline that returns a <see cref="Result{TContext}"/> with cancellation token support.
	/// Throws <see cref="Exceptions.PipelineException"/> if the result is failure.
	/// </summary>
	/// <param name="handler">Async step handler returning a <see cref="Result{TContext}"/>.</param>
	/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
	IPipelineBuilder<TContext> StepResultAsync(
		Func<TContext, CancellationToken, Task<Result<TContext>>> handler );
}
