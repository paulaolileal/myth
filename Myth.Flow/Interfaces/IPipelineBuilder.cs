using Myth.Models;

namespace Myth.Interfaces {

	/// <summary>
	/// Main pipeline builder interface for constructing and executing pipelines.
	/// Provides fluent methods for adding steps, transformations, taps, conditionals, telemetry, and retry logic.
	/// </summary>
	public interface IPipelineBuilder<TContext> {

		/// <summary>
		/// Adds a synchronous step to the pipeline using a service resolved from DI.
		/// </summary>
		/// <typeparam name="TService">Type of service to resolve.</typeparam>
		/// <param name="handler">Step handler function.</param>
		/// <param name="onSuccess">Optional callback on success.</param>
		/// <param name="onError">Optional error handler for this step.</param>
		/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
		IPipelineBuilder<TContext> Step<TService>(
			Func<TService, TContext, TContext> handler,
			Action<TContext>? onSuccess = null,
			Action<Exception>? onError = null )
			where TService : notnull;

		/// <summary>
		/// Adds an asynchronous step to the pipeline using a service resolved from DI.
		/// </summary>
		/// <typeparam name="TService">Type of service to resolve.</typeparam>
		/// <param name="handler">Async step handler function.</param>
		/// <param name="onSuccess">Optional callback on success.</param>
		/// <param name="onError">Optional error handler for this step.</param>
		/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
		IPipelineBuilder<TContext> StepAsync<TService>(
			Func<TService, TContext, Task<TContext>> handler,
			Action<TContext>? onSuccess = null,
			Action<Exception>? onError = null )
			where TService : notnull;

		/// <summary>
		/// Adds a synchronous step to the pipeline that returns a <see cref="Result{TContext}"/>.
		/// Throws <see cref="Exceptions.PipelineException"/> if the result is failure.
		/// </summary>
		/// <typeparam name="TService">Type of service to resolve.</typeparam>
		/// <param name="handler">Step handler returning a <see cref="Result{TContext}"/>.</param>
		/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
		IPipelineBuilder<TContext> StepResult<TService>(
			Func<TService, TContext, Result<TContext>> handler )
			where TService : notnull;

		/// <summary>
		/// Adds an asynchronous step to the pipeline that returns a <see cref="Result{TContext}"/>.
		/// Throws <see cref="Exceptions.PipelineException"/> if the result is failure.
		/// </summary>
		/// <typeparam name="TService">Type of service to resolve.</typeparam>
		/// <param name="handler">Async step handler returning a <see cref="Result{TContext}"/>.</param>
		/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
		IPipelineBuilder<TContext> StepResultAsync<TService>(
			Func<TService, TContext, Task<Result<TContext>>> handler )
			where TService : notnull;

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
		/// Adds a tap step to the pipeline that executes a side-effect action using a service resolved from DI.
		/// </summary>
		/// <typeparam name="TService">Type of service to resolve.</typeparam>
		/// <param name="action">Action to execute using the service and context.</param>
		/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
		IPipelineBuilder<TContext> Tap<TService>( Action<TService, TContext> action )
			where TService : notnull;

		/// <summary>
		/// Adds an asynchronous tap step to the pipeline that executes a side-effect async action using a service resolved from DI.
		/// </summary>
		/// <typeparam name="TService">Type of service to resolve.</typeparam>
		/// <param name="action">Async action to execute using the service and context.</param>
		/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
		IPipelineBuilder<TContext> TapAsync<TService>( Func<TService, TContext, Task> action )
			where TService : notnull;

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

		// Object-based pipeline methods (simplified API without context management)

		/// <summary>
		/// Adds an asynchronous step to the pipeline using a service resolved from DI with cancellation token support.
		/// The step receives the current object and a cancellation token, returns a new object directly.
		/// </summary>
		/// <typeparam name="TService">Type of service to resolve.</typeparam>
		/// <param name="handler">Async step handler function that takes the current object and cancellation token and returns a new object.</param>
		/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
		IPipelineBuilder<TContext> StepAsync<TService>(
			Func<TService, TContext, CancellationToken, Task<TContext>> handler )
			where TService : notnull;

		/// <summary>
		/// Adds an asynchronous step to the pipeline that returns a <see cref="Result{TContext}"/>.
		/// The step receives the current object and a cancellation token, returns a Result with the new object.
		/// Throws <see cref="Exceptions.PipelineException"/> if the result is failure.
		/// </summary>
		/// <typeparam name="TService">Type of service to resolve.</typeparam>
		/// <param name="handler">Async step handler returning a <see cref="Result{TContext}"/>.</param>
		/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
		IPipelineBuilder<TContext> StepResultAsync<TService>(
			Func<TService, TContext, CancellationToken, Task<Result<TContext>>> handler )
			where TService : notnull;
	}
}