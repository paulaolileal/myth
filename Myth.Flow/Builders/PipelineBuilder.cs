using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Myth.Exceptions;
using Myth.Interfaces;
using Myth.Models;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Myth.Builders {

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
		/// Adds a synchronous step to the pipeline using a service resolved from DI.
		/// </summary>
		/// <typeparam name="TService">Type of service to resolve.</typeparam>
		/// <param name="handler">Step handler function.</param>
		/// <param name="onSuccess">Optional callback on success.</param>
		/// <param name="onError">Optional error handler for this step.</param>
		/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
		/// <exception cref="PipelineConfigurationException">Thrown if service provider is not available or service is not registered.</exception>
		public IPipelineBuilder<TContext> Step<TService>(
			Func<TService, TContext, TContext> handler,
			Action<TContext>? onSuccess = null,
			Action<Exception>? onError = null )
			where TService : notnull {
			if ( onError != null )
				_errorHandlers.Add( onError );

			var stepName = typeof( TService ).Name;
			var currentRetry = _retryAttempts;
			var currentBackoff = _backoffMs;

			_steps.Add( new StepDescriptor<TContext>(
				StepType.Sync,
				async ( context, ct ) => {
					var service = GetRequiredService<TService>( );
					var result = handler( service, context );
					onSuccess?.Invoke( result );
					return await Task.FromResult( result );
				},
				stepName,
				currentRetry,
				currentBackoff ) );

			return this;
		}

		/// <summary>
		/// Adds an asynchronous step to the pipeline using a service resolved from DI.
		/// </summary>
		/// <typeparam name="TService">Type of service to resolve.</typeparam>
		/// <param name="handler">Async step handler function.</param>
		/// <param name="onSuccess">Optional callback on success.</param>
		/// <param name="onError">Optional error handler for this step.</param>
		/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
		/// <exception cref="PipelineConfigurationException">Thrown if service provider is not available or service is not registered.</exception>
		public IPipelineBuilder<TContext> StepAsync<TService>(
			Func<TService, TContext, Task<TContext>> handler,
			Action<TContext>? onSuccess = null,
			Action<Exception>? onError = null )
			where TService : notnull {
			if ( onError != null )
				_errorHandlers.Add( onError );

			var stepName = typeof( TService ).Name;
			var currentRetry = _retryAttempts;
			var currentBackoff = _backoffMs;

			_steps.Add( new StepDescriptor<TContext>(
				StepType.Async,
				async ( context, ct ) => {
					var service = GetRequiredService<TService>( );
					var result = await handler( service, context ).ConfigureAwait( false );
					onSuccess?.Invoke( result );
					return result;
				},
				stepName,
				currentRetry,
				currentBackoff ) );

			return this;
		}

		/// <summary>
		/// Adds a synchronous step to the pipeline that returns a <see cref="Result{TContext}"/>.
		/// Throws <see cref="PipelineException"/> if the result is failure.
		/// </summary>
		/// <typeparam name="TService">Type of service to resolve.</typeparam>
		/// <param name="handler">Step handler returning a <see cref="Result{TContext}"/>.</param>
		/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
		/// <exception cref="PipelineException">Thrown if the result is failure.</exception>
		/// <exception cref="PipelineConfigurationException">Thrown if service provider is not available or service is not registered.</exception>
		public IPipelineBuilder<TContext> StepResult<TService>(
			Func<TService, TContext, Result<TContext>> handler )
			where TService : notnull {
			var stepName = typeof( TService ).Name;
			var currentRetry = _retryAttempts;
			var currentBackoff = _backoffMs;

			_steps.Add( new StepDescriptor<TContext>(
				StepType.Sync,
				async ( context, ct ) => {
					var service = GetRequiredService<TService>( );
					var result = handler( service, context );

					if ( result.IsFailure ) {
						throw new PipelineException(
							result.ErrorMessage ?? "Step failed",
							result.Exception );
					}

					return await Task.FromResult( result.Value! );
				},
				stepName,
				currentRetry,
				currentBackoff ) );

			return this;
		}

		/// <summary>
		/// Adds an asynchronous step to the pipeline that returns a <see cref="Result{TContext}"/>
		/// Throws <see cref="PipelineException"/> if the result is failure.
		/// </summary>
		/// <typeparam name="TService">Type of service to resolve.</typeparam>
		/// <param name="handler">Async step handler returning a <see cref="Result{TContext}"/>.</param>
		/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
		/// <exception cref="PipelineException">Thrown if the result is failure.</exception>
		/// <exception cref="PipelineConfigurationException">Thrown if service provider is not available or service is not registered.</exception>
		public IPipelineBuilder<TContext> StepResultAsync<TService>(
			Func<TService, TContext, Task<Result<TContext>>> handler )
			where TService : notnull {
			var stepName = typeof( TService ).Name;
			var currentRetry = _retryAttempts;
			var currentBackoff = _backoffMs;

			_steps.Add( new StepDescriptor<TContext>(
				StepType.Async,
				async ( context, ct ) => {
					var service = GetRequiredService<TService>( );
					var result = await handler( service, context ).ConfigureAwait( false );

					if ( result.IsFailure ) {
						throw new PipelineException(
							result.ErrorMessage ?? "Step failed",
							result.Exception );
					}

					return result.Value!;
				},
				stepName,
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
			var newSteps = new List<StepDescriptor<TNewContext>>( _steps.Count + 1 ) {
				// Create a wrapper step that executes all previous steps then transforms
				new(
				StepType.Transform,
				async ( context, ct ) => {
					// Start with original input
					var currentContext = _input;

					// Execute all previous steps
					foreach ( var step in _steps ) {
						currentContext = await step.Handler( currentContext, ct );
					}

					// Transform the final result
					return await Task.FromResult( mapper( currentContext ) );
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
			var newSteps = new List<StepDescriptor<TNewContext>>( _steps.Count + 1 );

			// Create a wrapper step that executes all previous steps then transforms
			newSteps.Add( new StepDescriptor<TNewContext>(
				StepType.Transform,
				async ( context, ct ) => {
					// Start with original input
					var currentContext = _input;

					// Execute all previous steps
					foreach ( var step in _steps ) {
						currentContext = await step.Handler( currentContext, ct );
					}

					// Transform the final result
					return await mapper( currentContext ).ConfigureAwait( false );
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
		/// Adds a tap step to the pipeline that executes a side-effect action using a service resolved from DI.
		/// </summary>
		/// <typeparam name="TService">Type of service to resolve.</typeparam>
		/// <param name="action">Action to execute using the service and context.</param>
		/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
		/// <exception cref="PipelineConfigurationException">Thrown if service provider is not available or service is not registered.</exception>
		public IPipelineBuilder<TContext> Tap<TService>( Action<TService, TContext> action )
			where TService : notnull {
			var stepName = $"Tap_{typeof( TService ).Name}";

			_steps.Add( new StepDescriptor<TContext>(
				StepType.Tap,
				async ( context, ct ) => {
					var service = GetRequiredService<TService>( );
					action( service, context );
					return await Task.FromResult( context );
				},
				stepName,
				0,
				0 ) );

			return this;
		}

		/// <summary>
		/// Adds an asynchronous tap step to the pipeline that executes a side-effect async action using a service resolved from DI.
		/// </summary>
		/// <typeparam name="TService">Type of service to resolve.</typeparam>
		/// <param name="action">Async action to execute using the service and context.</param>
		/// <returns>The current <see cref="IPipelineBuilder{TContext}"/> instance.</returns>
		/// <exception cref="PipelineConfigurationException">Thrown if service provider is not available or service is not registered.</exception>
		public IPipelineBuilder<TContext> TapAsync<TService>( Func<TService, TContext, Task> action )
			where TService : notnull {
			var stepName = $"TapAsync_{typeof( TService ).Name}";

			_steps.Add( new StepDescriptor<TContext>(
				StepType.Tap,
				async ( context, ct ) => {
					var service = GetRequiredService<TService>( );
					await action( service, context ).ConfigureAwait( false );
					return context;
				},
				stepName,
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
				var stepIndex = 0;

				foreach ( var step in _steps ) {
					cancellationToken.ThrowIfCancellationRequested( );

					using var stepActivity = activity?.Source?.StartActivity(
						$"Step_{stepIndex}_{step.Name ?? "Unknown"}" );

					context = await ExecuteStepWithRetryAsync(
						step,
						context,
						cancellationToken,
						logger );

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
			} catch ( Exception ex ) {
				activity?.SetStatus( ActivityStatusCode.Error, ex.Message );

				logger?.LogError( ex, "Pipeline execution failed" );

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
		/// Resolves a required service from the DI container.
		/// </summary>
		/// <typeparam name="TService">Type of service to resolve.</typeparam>
		/// <returns>The resolved service instance.</returns>
		/// <exception cref="PipelineConfigurationException">Thrown if service provider is not available or service is not registered.</exception>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private TService GetRequiredService<TService>( ) where TService : notnull {
			if ( _serviceProvider is null )
				throw new PipelineConfigurationException(
					$"ServiceProvider is not available. " +
					$"Ensure IServiceProvider is provided to Pipeline.Start() or use services.AddFlow()" );

			try {
				var service = _serviceProvider.GetRequiredService<TService>( );

				return service;
			} catch ( InvalidOperationException ex ) {
				// Wrap DI exception in PipelineConfigurationException
				throw new PipelineConfigurationException(
					$"Service {typeof( TService ).Name} not registered. " +
					$"Ensure the service is registered in the dependency injection container.",
					ex );
			}
		}

		/// <summary>
		/// Executes a pipeline step with retry logic according to the step's configuration.
		/// </summary>
		/// <param name="step">Step descriptor to execute.</param>
		/// <param name="context">Current pipeline context.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <param name="logger">Optional logger for diagnostics.</param>
		/// <returns>The resulting context after step execution.</returns>
		private async Task<TContext> ExecuteStepWithRetryAsync(
			StepDescriptor<TContext> step,
			TContext context,
			CancellationToken cancellationToken,
			ILogger? logger ) {
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
				} catch ( Exception ex ) when ( attempts < step.RetryAttempts ) {
					attempts++;

					logger?.LogWarning(
						ex,
						"Step {StepName} failed (attempt {Attempt}/{MaxAttempts}). Retrying...",
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
	}
}