using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Myth.Exceptions;
using Myth.Interfaces;
using Myth.Models;

namespace Myth.Builders;

internal sealed class PipelineBuilder<TContext> : IPipelineBuilder<TContext> {
	private readonly TContext _input;
	private readonly IServiceProvider? _serviceProvider;
	private readonly PipelineConfiguration _configuration;
	private readonly List<StepDescriptor<TContext>> _steps;
	private readonly List<Action<Exception>> _errorHandlers;
	private string? _telemetryOperationName;
	private int _retryAttempts;
	private int _backoffMs;

	/// <summary>
	/// Initializes a new instance of <see cref="PipelineBuilder{TContext}"/> with the specified input, service provider and configuration.
	/// </summary>
	/// <param name="input">Initial pipeline context.</param>
	/// <param name="serviceProvider">Optional DI service provider for resolving dependencies.</param>
	/// <param name="configuration">Pipeline configuration (<see cref="PipelineConfiguration"/>).</param>
	internal PipelineBuilder(
		TContext input,
		IServiceProvider? serviceProvider,
		PipelineConfiguration configuration ) {
		_input = input;
		_serviceProvider = serviceProvider;
		_configuration = configuration;
		_steps = new List<StepDescriptor<TContext>>( 4 );
		_errorHandlers = new List<Action<Exception>>( 2 );
		_retryAttempts = configuration.DefaultRetryAttempts;
		_backoffMs = configuration.DefaultBackoffMs;
	}

	/// <summary>
	/// Internal constructor for cloning or transforming pipeline state.
	/// </summary>
	/// <param name="input">Initial pipeline context.</param>
	/// <param name="serviceProvider">DI service provider.</param>
	/// <param name="configuration">Pipeline configuration.</param>
	/// <param name="steps">List of pipeline steps.</param>
	/// <param name="errorHandlers">List of error handlers.</param>
	/// <param name="telemetryOperationName">Telemetry operation name.</param>
	/// <param name="retryAttempts">Retry attempts for steps.</param>
	/// <param name="backoffMs">Backoff in milliseconds for retries.</param>
	private PipelineBuilder(
		TContext input,
		IServiceProvider? serviceProvider,
		PipelineConfiguration configuration,
		List<StepDescriptor<TContext>> steps,
		List<Action<Exception>> errorHandlers,
		string? telemetryOperationName,
		int retryAttempts,
		int backoffMs ) {
		_input = input;
		_serviceProvider = serviceProvider;
		_configuration = configuration;
		_steps = steps;
		_errorHandlers = errorHandlers;
		_telemetryOperationName = telemetryOperationName;
		_retryAttempts = retryAttempts;
		_backoffMs = backoffMs;
	}

	/// <summary>
	/// Adds a synchronous step to the pipeline.
	/// </summary>
	/// <param name="handler">Step handler function that takes the current context and returns a new context.</param>
	/// <param name="onSuccess">Optional callback on success.</param>
	/// <param name="onError">Optional error handler for this step.</param>
	/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
	public IPipelineBuilder<TContext> Step(
		Func<TContext, TContext> handler,
		Action<TContext>? onSuccess = null,
		Action<Exception>? onError = null ) {
		if ( onError != null )
			_errorHandlers.Add( onError );

		var currentRetry = _retryAttempts;
		var currentBackoff = _backoffMs;

		_steps.Add( new StepDescriptor<TContext>(
			StepType.Sync,
			async ( context, ct ) => {
				var result = handler( context );
				onSuccess?.Invoke( result );
				return await Task.FromResult( result );
			},
			"Step",
			currentRetry,
			currentBackoff ) );

		return this;
	}

	/// <summary>
	/// Adds an asynchronous step to the pipeline.
	/// </summary>
	/// <param name="handler">Async step handler function that takes the current context and returns a new context.</param>
	/// <param name="onSuccess">Optional callback on success.</param>
	/// <param name="onError">Optional error handler for this step.</param>
	/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
	public IPipelineBuilder<TContext> StepAsync(
		Func<TContext, Task<TContext>> handler,
		Action<TContext>? onSuccess = null,
		Action<Exception>? onError = null ) {
		if ( onError != null )
			_errorHandlers.Add( onError );

		var currentRetry = _retryAttempts;
		var currentBackoff = _backoffMs;

		_steps.Add( new StepDescriptor<TContext>(
			StepType.Async,
			async ( context, ct ) => {
				var result = await handler( context ).ConfigureAwait( false );
				onSuccess?.Invoke( result );
				return result;
			},
			"StepAsync",
			currentRetry,
			currentBackoff ) );

		return this;
	}

	/// <summary>
	/// Adds an asynchronous step to the pipeline with cancellation token support.
	/// </summary>
	/// <param name="handler">Async step handler function that takes the current context and cancellation token, returns a new context.</param>
	/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
	public IPipelineBuilder<TContext> StepAsync(
		Func<TContext, CancellationToken, Task<TContext>> handler ) {
		var currentRetry = _retryAttempts;
		var currentBackoff = _backoffMs;

		_steps.Add( new StepDescriptor<TContext>(
			StepType.Async,
			async ( context, ct ) => {
				var result = await handler( context, ct ).ConfigureAwait( false );
				return result;
			},
			"StepAsync",
			currentRetry,
			currentBackoff ) );

		return this;
	}

	/// <summary>
	/// Adds a synchronous step to the pipeline that returns a <see cref="Result{TContext}"/>.
	/// Throws <see cref="PipelineException"/> if the result is failure.
	/// </summary>
	/// <param name="handler">Step handler returning a <see cref="Result{TContext}"/>.</param>
	/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
	/// <exception cref="PipelineException">Thrown if the result is failure.</exception>
	public IPipelineBuilder<TContext> StepResult(
		Func<TContext, Result<TContext>> handler ) {
		var currentRetry = _retryAttempts;
		var currentBackoff = _backoffMs;

		_steps.Add( new StepDescriptor<TContext>(
			StepType.Sync,
			async ( context, ct ) => {
				var result = handler( context );

				if ( result.IsFailure ) {
					throw new PipelineException(
						result.ErrorMessage ?? "Step failed",
						result.Exception );
				}

				return await Task.FromResult( result.Value! );
			},
			"StepResult",
			currentRetry,
			currentBackoff ) );

		return this;
	}

	/// <summary>
	/// Adds an asynchronous step to the pipeline that returns a <see cref="Result{TContext}"/>.
	/// Throws <see cref="PipelineException"/> if the result is failure.
	/// </summary>
	/// <param name="handler">Async step handler returning a <see cref="Result{TContext}"/>.</param>
	/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
	/// <exception cref="PipelineException">Thrown if the result is failure.</exception>
	public IPipelineBuilder<TContext> StepResultAsync(
		Func<TContext, Task<Result<TContext>>> handler ) {
		var currentRetry = _retryAttempts;
		var currentBackoff = _backoffMs;

		_steps.Add( new StepDescriptor<TContext>(
			StepType.Async,
			async ( context, ct ) => {
				var result = await handler( context ).ConfigureAwait( false );

				if ( result.IsFailure ) {
					throw new PipelineException(
						result.ErrorMessage ?? "Step failed",
						result.Exception );
				}

				return result.Value!;
			},
			"StepResultAsync",
			currentRetry,
			currentBackoff ) );

		return this;
	}

	/// <summary>
	/// Adds an asynchronous step to the pipeline that returns a <see cref="Result{TContext}"/> with cancellation token support.
	/// Throws <see cref="PipelineException"/> if the result is failure.
	/// </summary>
	/// <param name="handler">Async step handler returning a <see cref="Result{TContext}"/>.</param>
	/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
	/// <exception cref="PipelineException">Thrown if the result is failure.</exception>
	public IPipelineBuilder<TContext> StepResultAsync(
		Func<TContext, CancellationToken, Task<Result<TContext>>> handler ) {
		var currentRetry = _retryAttempts;
		var currentBackoff = _backoffMs;

		_steps.Add( new StepDescriptor<TContext>(
			StepType.Async,
			async ( context, ct ) => {
				var result = await handler( context, ct ).ConfigureAwait( false );

				if ( result.IsFailure ) {
					throw new PipelineException(
						result.ErrorMessage ?? "Step failed",
						result.Exception );
				}

				return result.Value!;
			},
			"StepResultAsync",
			currentRetry,
			currentBackoff ) );

		return this;
	}

	/// <summary>
	/// Transforms the pipeline context to a new type using the provided mapper function.
	/// All previous steps are executed before transformation.
	/// </summary>
	/// <typeparam name="TNewContext">Type of the new context.</typeparam>
	/// <param name="mapper">Function to map the old context to the new context.</param>
	/// <returns>A new <see cref="IPipelineBuilder{TNewContext}"/> instance.</returns>
	public IPipelineBuilder<TNewContext> Transform<TNewContext>(
		Func<TContext, TNewContext> mapper ) {
		var capturedSteps = _steps.ToList( );
		var capturedConfiguration = _configuration;
		var newSteps = new List<StepDescriptor<TNewContext>>( capturedSteps.Count + 1 ) {
			// Create a wrapper step that executes all previous steps then transforms
			new(
			StepType.Transform,
			async ( context, ct ) => {
				// Start with original input
				var currentContext = _input;

				// Execute all previous steps, identifying which one fails for better diagnostics
				var innerStepIndex = 0;
				foreach ( var step in capturedSteps ) {
					try {
						currentContext = await step.Handler( currentContext, ct );
					} catch ( Exception ex ) when (
						capturedConfiguration.ExceptionTypesToPropagate.Count > 0 &&
						capturedConfiguration.ExceptionTypesToPropagate.Any( t => t.IsAssignableFrom( ex.GetType( ) ) ) ) {
						throw;
					} catch ( Exception ex ) {
						throw new PipelineException(
							$"Transform failed while re-executing inner step [{innerStepIndex}] '{step.Name ?? "Unknown"}': {ex.Message}",
							ex );
					}
					innerStepIndex++;
				}

				// Transform the final result
				try {
					return await Task.FromResult( mapper( currentContext ) );
				} catch ( Exception ex ) when (
					capturedConfiguration.ExceptionTypesToPropagate.Count > 0 &&
					capturedConfiguration.ExceptionTypesToPropagate.Any( t => t.IsAssignableFrom( ex.GetType( ) ) ) ) {
					throw;
				} catch ( Exception ex ) {
					throw new PipelineException(
						$"Transform failed during mapping phase ({typeof( TContext ).Name} -> {typeof( TNewContext ).Name}): {ex.Message}",
						ex );
				}
			},
			"Transform",
			0,
			0 )
		};

		return new PipelineBuilder<TNewContext>(
			default( TNewContext )!,
			_serviceProvider,
			_configuration,
			newSteps,
			_errorHandlers,
			_telemetryOperationName,
			_retryAttempts,
			_backoffMs );
	}

	/// <summary>
	/// Asynchronously transforms the pipeline context to a new type using the provided async mapper function.
	/// All previous steps are executed before transformation.
	/// </summary>
	/// <typeparam name="TNewContext">Type of the new context.</typeparam>
	/// <param name="mapper">Async function to map the old context to the new context.</param>
	/// <returns>A new <see cref="IPipelineBuilder{TNewContext}"/> instance.</returns>
	public IPipelineBuilder<TNewContext> TransformAsync<TNewContext>(
		Func<TContext, Task<TNewContext>> mapper ) {
		var capturedSteps = _steps.ToList( );
		var capturedConfiguration = _configuration;
		var newSteps = new List<StepDescriptor<TNewContext>>( capturedSteps.Count + 1 );

		// Create a wrapper step that executes all previous steps then transforms
		newSteps.Add( new StepDescriptor<TNewContext>(
			StepType.Transform,
			async ( context, ct ) => {
				// Start with original input
				var currentContext = _input;

				// Execute all previous steps, identifying which one fails for better diagnostics
				var innerStepIndex = 0;
				foreach ( var step in capturedSteps ) {
					try {
						currentContext = await step.Handler( currentContext, ct );
					} catch ( Exception ex ) when (
						capturedConfiguration.ExceptionTypesToPropagate.Count > 0 &&
						capturedConfiguration.ExceptionTypesToPropagate.Any( t => t.IsAssignableFrom( ex.GetType( ) ) ) ) {
						throw;
					} catch ( Exception ex ) {
						throw new PipelineException(
							$"TransformAsync failed while re-executing inner step [{innerStepIndex}] '{step.Name ?? "Unknown"}': {ex.Message}",
							ex );
					}
					innerStepIndex++;
				}

				// Transform the final result
				try {
					return await mapper( currentContext ).ConfigureAwait( false );
				} catch ( Exception ex ) when (
					capturedConfiguration.ExceptionTypesToPropagate.Count > 0 &&
					capturedConfiguration.ExceptionTypesToPropagate.Any( t => t.IsAssignableFrom( ex.GetType( ) ) ) ) {
					throw;
				} catch ( Exception ex ) {
					throw new PipelineException(
						$"TransformAsync failed during mapping phase ({typeof( TContext ).Name} -> {typeof( TNewContext ).Name}): {ex.Message}",
						ex );
				}
			},
			"TransformAsync",
			0,
			0 ) );

		return new PipelineBuilder<TNewContext>(
			default( TNewContext )!,
			_serviceProvider,
			_configuration,
			newSteps,
			_errorHandlers,
			_telemetryOperationName,
			_retryAttempts,
			_backoffMs );
	}

	/// <summary>
	/// Adds a tap step to the pipeline that executes a side-effect action on the context.
	/// </summary>
	/// <param name="action">Action to execute on the context.</param>
	/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
	public IPipelineBuilder<TContext> Tap( Action<TContext> action ) {
		_steps.Add( new StepDescriptor<TContext>(
			StepType.Tap,
			async ( context, ct ) => {
				action( context );
				return await Task.FromResult( context );
			},
			"Tap",
			0,
			0 ) );

		return this;
	}

	/// <summary>
	/// Adds an asynchronous tap step to the pipeline that executes a side-effect async action on the context.
	/// </summary>
	/// <param name="action">Async action to execute on the context.</param>
	/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
	public IPipelineBuilder<TContext> TapAsync( Func<TContext, Task> action ) {
		_steps.Add( new StepDescriptor<TContext>(
			StepType.Tap,
			async ( context, ct ) => {
				await action( context ).ConfigureAwait( false );
				return context;
			},
			"TapAsync",
			0,
			0 ) );

		return this;
	}

	/// <summary>
	/// Adds a conditional step to the pipeline. Executes the configured pipeline if the predicate returns true.
	/// </summary>
	/// <param name="predicate">Predicate to determine if the conditional pipeline should run.</param>
	/// <param name="configurePipeline">Action to configure the conditional pipeline.</param>
	/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
	/// <exception cref="PipelineException">Thrown if the conditional pipeline fails.</exception>
	public IPipelineBuilder<TContext> When(
		Func<TContext, bool> predicate,
		Action<IPipelineBuilder<TContext>> configurePipeline ) {
		_steps.Add( new StepDescriptor<TContext>(
			StepType.Conditional,
			async ( context, ct ) => {
				if ( predicate( context ) ) {
					var conditionalBuilder = new PipelineBuilder<TContext>(
						context,
						_serviceProvider,
						_configuration );

					configurePipeline( conditionalBuilder );

					var result = await conditionalBuilder.ExecuteAsync( ct );

					if ( result.IsFailure ) {
						throw new PipelineException(
							result.ErrorMessage ?? "Conditional step failed",
							result.Exception );
					}

					return result.Value!;
				}

				return context;
			},
			"When",
			0,
			0 ) );

		return this;
	}

	/// <summary>
	/// Enables telemetry for the pipeline execution with the specified operation name.
	/// </summary>
	/// <param name="operationName">Telemetry operation name.</param>
	/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
	public IPipelineBuilder<TContext> WithTelemetry( string operationName ) {
		_telemetryOperationName = operationName;

		return this;
	}

	/// <summary>
	/// Configures retry attempts and backoff for subsequent steps in the pipeline.
	/// </summary>
	/// <param name="maxAttempts">Maximum number of retry attempts.</param>
	/// <param name="backoffMs">Backoff in milliseconds between retries.</param>
	/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
	public IPipelineBuilder<TContext> WithRetry( int maxAttempts, int backoffMs = 100 ) {
		_retryAttempts = maxAttempts;
		_backoffMs = backoffMs;

		return this;
	}

	/// <summary>
	/// Executes the pipeline asynchronously, running all configured steps in order.
	/// </summary>
	/// <param name="cancellationToken">Optional cancellation token.</param>
	/// <returns>A <see cref="Result{TContext}"/> representing the outcome of the pipeline execution.</returns>
	/// <exception cref="PipelineConfigurationException">Thrown for configuration errors.</exception>
	public async Task<Result<TContext>> ExecuteAsync(
		CancellationToken cancellationToken = default ) {
		Activity? activity = null;
		var logger = _serviceProvider?.GetService<ILogger<PipelineBuilder<TContext>>>( );
		var stepIndex = 0;
		StepDescriptor<TContext>? currentStep = null;

		try {
			// Resolve ActivitySource first
			ActivitySource? activitySource = null;
			if ( _configuration.EnableTelemetry ) {
				activitySource = _configuration.ActivitySource
					?? _serviceProvider?.GetService<ActivitySource>( )
					?? new ActivitySource( "Myth.Flow" );
			}

			if ( _configuration.EnableTelemetry && _telemetryOperationName != null && activitySource != null ) {
				activity = activitySource.StartActivity( _telemetryOperationName );

				activity?.SetTag( "pipeline.input.type", typeof( TContext ).Name );
			}

			logger?.LogInformation(
				"Starting pipeline execution with {StepCount} steps",
				_steps.Count );

			var context = _input;

			foreach ( var step in _steps ) {
				cancellationToken.ThrowIfCancellationRequested( );

				currentStep = step;

				using var stepActivity = activity?.Source?.StartActivity(
					$"Step_{stepIndex}_{step.Name ?? "Unknown"}" );

				context = await ExecuteStepWithRetryAsync(
					step,
					context,
					cancellationToken,
					logger,
					stepIndex );

				stepIndex++;
			}

			activity?.SetStatus( ActivityStatusCode.Ok );

			logger?.LogInformation( "Pipeline execution completed successfully" );

			return Result<TContext>.Success( context );
		} catch ( OperationCanceledException ) {
			activity?.SetStatus( ActivityStatusCode.Error, "Operation cancelled" );

			logger?.LogWarning( "Pipeline execution was cancelled" );

			return Result<TContext>.Failure( "Operation was cancelled" );
		} catch ( PipelineConfigurationException ) {
			// Always re-throw configuration exceptions (fail-fast)
			throw;
		} catch ( Exception ex ) when ( ShouldPropagateException( ex ) ) {
			// Re-throw exceptions that should be propagated without handling
			activity?.SetStatus( ActivityStatusCode.Error, ex.Message );

			logger?.LogError( ex,
				"Pipeline execution failed at step [{StepIndex}] '{StepName}' with propagated exception",
				stepIndex, currentStep?.Name ?? "Unknown" );

			throw;
		} catch ( Exception ex ) {
			activity?.SetStatus( ActivityStatusCode.Error, ex.Message );

			logger?.LogError( ex,
				"Pipeline execution failed at step [{StepIndex}] '{StepName}'",
				stepIndex, currentStep?.Name ?? "Unknown" );

			foreach ( var handler in _errorHandlers ) {
				try {
					handler( ex );
				} catch ( Exception handlerEx ) {
					logger?.LogError(
						handlerEx,
						"Error handler threw an exception" );
				}
			}

			return Result<TContext>.Failure( ex.Message, ex );
		} finally {
			activity?.Dispose( );
		}
	}

	/// <summary>
	/// Executes the pipeline synchronously, running all configured steps in order.
	/// </summary>
	/// <returns>A <see cref="Result{TContext}"/> representing the outcome of the pipeline execution.</returns>
	public Result<TContext> Execute( ) =>
		ExecuteAsync( )
			.GetAwaiter( )
			.GetResult( );

	/// <summary>
	/// Executes a pipeline step with retry logic according to the step's configuration.
	/// </summary>
	/// <param name="step">Step descriptor to execute.</param>
	/// <param name="context">Current pipeline context.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <param name="logger">Optional logger for diagnostics.</param>
	/// <param name="stepIndex">Index of the step in the pipeline for diagnostic purposes.</param>
	/// <returns>The resulting context after step execution.</returns>
	private async Task<TContext> ExecuteStepWithRetryAsync(
		StepDescriptor<TContext> step,
		TContext context,
		CancellationToken cancellationToken,
		ILogger? logger,
		int stepIndex = 0 ) {
		var attempts = 0;
		var maxAttempts = step.RetryAttempts + 1;

		while ( attempts < maxAttempts ) {
			try {
				return await step
					.Handler( context, cancellationToken )
					.ConfigureAwait( false );
			} catch ( OperationCanceledException ) {
				// Re-throw cancellation without retry
				throw;
			} catch ( Exception ex ) when ( ShouldPropagateException( ex ) ) {
				// Re-throw exceptions that should be propagated without handling
				throw;
			} catch ( Exception ex ) when ( attempts < step.RetryAttempts ) {
				attempts++;

				logger?.LogWarning(
					ex,
					"Step [{StepIndex}] '{StepName}' failed (attempt {Attempt}/{MaxAttempts}). Retrying...",
					stepIndex,
					step.Name,
					attempts,
					maxAttempts );

				await Task
					.Delay( step.BackoffMs * attempts, cancellationToken )
					.ConfigureAwait( false );
			}
		}

		// Final attempt without catching
		return await step
			.Handler( context, cancellationToken )
			.ConfigureAwait( false );
	}

	/// <summary>
	/// Determines whether an exception should be propagated based on the configured exception filter.
	/// </summary>
	/// <param name="exception">The exception to evaluate.</param>
	/// <returns>True if the exception should be propagated; otherwise, false.</returns>
	private bool ShouldPropagateException( Exception exception ) {
		if ( _configuration.ExceptionTypesToPropagate.Count == 0 ) {
			return false;
		}

		var exceptionType = exception.GetType( );

		// Check if the exception type or any of its base types should be propagated
		return _configuration.ExceptionTypesToPropagate.Any( configuredType =>
			configuredType.IsAssignableFrom( exceptionType ) );
	}
}
