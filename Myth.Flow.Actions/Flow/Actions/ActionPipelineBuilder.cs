using Myth.Flow.Actions.Interfaces;
using Myth.Interfaces;
using Myth.Models;

namespace Myth.Flow.Actions;

/// <summary>
/// Internal implementation of action pipeline builder
/// </summary>
/// <typeparam name="TCurrent">The current request type in the pipeline</typeparam>
internal class ActionPipelineBuilder<TCurrent> : IActionPipelineBuilder<TCurrent> {
	private readonly IPipelineBuilder<ActionPipelineState<TCurrent>> _innerPipeline;

	/// <summary>
	/// Initializes a new instance of ActionPipelineBuilder
	/// </summary>
	/// <param name="innerPipeline">The underlying Myth.Flow pipeline</param>
	public ActionPipelineBuilder( IPipelineBuilder<ActionPipelineState<TCurrent>> innerPipeline ) {
		_innerPipeline = innerPipeline;
	}

	/// <summary>
	/// Gets the underlying Myth.Flow pipeline for extending functionality
	/// </summary>
	internal IPipelineBuilder<ActionPipelineState<TCurrent>> InnerPipeline => _innerPipeline;

	/// <summary>
	/// Adds a synchronous step to the pipeline.
	/// </summary>
	/// <param name="handler">Step handler function that takes the current state and returns a new state.</param>
	/// <param name="onSuccess">Optional callback on success.</param>
	/// <param name="onError">Optional error handler for this step.</param>
	/// <returns>The current <see cref="IActionPipelineBuilder{TCurrent}"/> instance.</returns>
	public IActionPipelineBuilder<TCurrent> Step(
		Func<ActionPipelineState<TCurrent>, ActionPipelineState<TCurrent>> handler,
		Action<ActionPipelineState<TCurrent>>? onSuccess = null,
		Action<Exception>? onError = null ) {
		var newPipeline = _innerPipeline.Step( handler, onSuccess, onError );
		return new ActionPipelineBuilder<TCurrent>( newPipeline );
	}

	/// <summary>
	/// Adds an asynchronous step to the pipeline.
	/// </summary>
	/// <param name="handler">Async step handler function that takes the current state and returns a new state.</param>
	/// <param name="onSuccess">Optional callback on success.</param>
	/// <param name="onError">Optional error handler for this step.</param>
	/// <returns>The current <see cref="IActionPipelineBuilder{TCurrent}"/> instance.</returns>
	public IActionPipelineBuilder<TCurrent> StepAsync(
		Func<ActionPipelineState<TCurrent>, Task<ActionPipelineState<TCurrent>>> handler,
		Action<ActionPipelineState<TCurrent>>? onSuccess = null,
		Action<Exception>? onError = null ) {
		var newPipeline = _innerPipeline.StepAsync( handler, onSuccess, onError );
		return new ActionPipelineBuilder<TCurrent>( newPipeline );
	}

	/// <summary>
	/// Adds an asynchronous step to the pipeline with cancellation token support.
	/// </summary>
	/// <param name="handler">Async step handler function that takes the current state and cancellation token, returns a new state.</param>
	/// <returns>The current <see cref="IActionPipelineBuilder{TCurrent}"/> instance.</returns>
	public IActionPipelineBuilder<TCurrent> StepAsync(
		Func<ActionPipelineState<TCurrent>, CancellationToken, Task<ActionPipelineState<TCurrent>>> handler ) {
		var newPipeline = _innerPipeline.StepAsync( handler );
		return new ActionPipelineBuilder<TCurrent>( newPipeline );
	}

	/// <summary>
	/// Adds a synchronous step to the pipeline that returns a <see cref="Result{T}"/>.
	/// Throws <see cref="Exceptions.PipelineException"/> if the result is failure.
	/// </summary>
	/// <param name="handler">Step handler returning a <see cref="Result{T}"/>.</param>
	/// <returns>The current <see cref="IActionPipelineBuilder{TCurrent}"/> instance.</returns>
	public IActionPipelineBuilder<TCurrent> StepResult(
		Func<ActionPipelineState<TCurrent>, Result<ActionPipelineState<TCurrent>>> handler ) {
		var newPipeline = _innerPipeline.StepResult( handler );
		return new ActionPipelineBuilder<TCurrent>( newPipeline );
	}

	/// <summary>
	/// Adds an asynchronous step to the pipeline that returns a <see cref="Result{T}"/>.
	/// Throws <see cref="Exceptions.PipelineException"/> if the result is failure.
	/// </summary>
	/// <param name="handler">Async step handler returning a <see cref="Result{T}"/>.</param>
	/// <returns>The current <see cref="IActionPipelineBuilder{TCurrent}"/> instance.</returns>
	public IActionPipelineBuilder<TCurrent> StepResultAsync(
		Func<ActionPipelineState<TCurrent>, Task<Result<ActionPipelineState<TCurrent>>>> handler ) {
		var newPipeline = _innerPipeline.StepResultAsync( handler );
		return new ActionPipelineBuilder<TCurrent>( newPipeline );
	}

	/// <summary>
	/// Adds an asynchronous step to the pipeline that returns a <see cref="Result{T}"/> with cancellation token support.
	/// Throws <see cref="Exceptions.PipelineException"/> if the result is failure.
	/// </summary>
	/// <param name="handler">Async step handler returning a <see cref="Result{T}"/>.</param>
	/// <returns>The current <see cref="IActionPipelineBuilder{TCurrent}"/> instance.</returns>
	public IActionPipelineBuilder<TCurrent> StepResultAsync(
		Func<ActionPipelineState<TCurrent>, CancellationToken, Task<Result<ActionPipelineState<TCurrent>>>> handler ) {
		var newPipeline = _innerPipeline.StepResultAsync( handler );
		return new ActionPipelineBuilder<TCurrent>( newPipeline );
	}

	/// <summary>
	/// Adds a tap step to the pipeline that executes a side-effect action on the context.
	/// </summary>
	/// <param name="action">Action to execute on the context.</param>
	/// <returns>The current <see cref="IActionPipelineBuilder{TCurrent}"/> instance.</returns>
	public IActionPipelineBuilder<TCurrent> Tap( Action<ActionPipelineState<TCurrent>> action ) {
		var newPipeline = _innerPipeline.Tap( action );
		return new ActionPipelineBuilder<TCurrent>( newPipeline );
	}

	/// <summary>
	/// Adds an asynchronous tap step to the pipeline that executes a side-effect async action on the context.
	/// </summary>
	/// <param name="action">Async action to execute on the context.</param>
	/// <returns>The current <see cref="IActionPipelineBuilder{TCurrent}"/> instance.</returns>
	public IActionPipelineBuilder<TCurrent> TapAsync( Func<ActionPipelineState<TCurrent>, Task> action ) {
		var newPipeline = _innerPipeline.TapAsync( action );
		return new ActionPipelineBuilder<TCurrent>( newPipeline );
	}

	/// <summary>
	/// Adds a conditional step to the pipeline. Executes the configured pipeline if the predicate returns true.
	/// </summary>
	/// <param name="predicate">Predicate to determine if the conditional pipeline should run.</param>
	/// <param name="configurePipeline">Action to configure the conditional pipeline.</param>
	/// <returns>The current <see cref="IActionPipelineBuilder{TCurrent}"/> instance.</returns>
	public IActionPipelineBuilder<TCurrent> When(
		Func<ActionPipelineState<TCurrent>, bool> predicate,
		Action<IActionPipelineBuilder<TCurrent>> configurePipeline ) {
		var newPipeline = _innerPipeline.When( predicate, innerBuilder => {
			var actionBuilder = new ActionPipelineBuilder<TCurrent>( innerBuilder );
			configurePipeline( actionBuilder );
		} );

		return new ActionPipelineBuilder<TCurrent>( newPipeline );
	}

	/// <summary>
	/// Enables telemetry for the pipeline execution with the specified operation name.
	/// </summary>
	/// <param name="operationName">Telemetry operation name.</param>
	/// <returns>The current <see cref="IActionPipelineBuilder{TCurrent}"/> instance.</returns>
	public IActionPipelineBuilder<TCurrent> WithTelemetry( string operationName ) {
		var newPipeline = _innerPipeline.WithTelemetry( operationName );
		return new ActionPipelineBuilder<TCurrent>( newPipeline );
	}

	/// <summary>
	/// Configures retry attempts and backoff for subsequent steps in the pipeline.
	/// </summary>
	/// <param name="maxAttempts">Maximum number of retry attempts.</param>
	/// <param name="backoffMs">Backoff in milliseconds between retries.</param>
	/// <returns>The current <see cref="IActionPipelineBuilder{TCurrent}"/> instance.</returns>
	public IActionPipelineBuilder<TCurrent> WithRetry( int maxAttempts, int backoffMs = 100 ) {
		var newPipeline = _innerPipeline.WithRetry( maxAttempts, backoffMs );
		return new ActionPipelineBuilder<TCurrent>( newPipeline );
	}

	/// <summary>
	/// Executes the pipeline and returns the result
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Pipeline execution result</returns>
	public async Task<Result<TCurrent>> ExecuteAsync( CancellationToken cancellationToken = default ) {
		var result = await _innerPipeline.ExecuteAsync( cancellationToken );

		if ( result.IsFailure ) {
			return Result<TCurrent>.Failure( result.ErrorMessage!, result.Exception );
		}

		var finalRequest = result.Value!.CurrentRequest;
		if ( finalRequest == null ) {
			return Result<TCurrent>.Failure( "Pipeline completed without a current request" );
		}

		return Result<TCurrent>.Success( finalRequest );
	}
}
