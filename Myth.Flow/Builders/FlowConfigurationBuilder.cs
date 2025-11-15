using System.Diagnostics;
using Myth.Models;

namespace Myth.Builders;

/// <summary>
/// Provides a fluent interface for configuring Myth.Flow pipeline options.
/// Allows chaining configuration methods for telemetry, logging, retry, and activity source settings.
/// </summary>
public sealed class FlowConfigurationBuilder {
	private readonly PipelineConfiguration _configuration;

	/// <summary>
	/// Initializes a new instance of the <see cref="FlowConfigurationBuilder"/> class.
	/// </summary>
	public FlowConfigurationBuilder( ) {
		_configuration = new PipelineConfiguration( );
	}

	/// <summary>
	/// Enables telemetry for pipeline execution.
	/// </summary>
	/// <returns>The current <see cref="FlowConfigurationBuilder"/> instance for method chaining.</returns>
	public FlowConfigurationBuilder UseTelemetry( ) {
		_configuration.EnableTelemetry = true;

		return this;
	}

	/// <summary>
	/// Disables telemetry for pipeline execution.
	/// </summary>
	/// <returns>The current <see cref="FlowConfigurationBuilder"/> instance for method chaining.</returns>
	public FlowConfigurationBuilder DisableTelemetry( ) {
		_configuration.EnableTelemetry = false;

		return this;
	}

	/// <summary>
	/// Enables logging for pipeline execution.
	/// </summary>
	/// <returns>The current <see cref="FlowConfigurationBuilder"/> instance for method chaining.</returns>
	public FlowConfigurationBuilder UseLogging( ) {
		_configuration.EnableLogging = true;

		return this;
	}

	/// <summary>
	/// Disables logging for pipeline execution.
	/// </summary>
	/// <returns>The current <see cref="FlowConfigurationBuilder"/> instance for method chaining.</returns>
	public FlowConfigurationBuilder DisableLogging( ) {
		_configuration.EnableLogging = false;

		return this;
	}

	/// <summary>
	/// Configures the default retry policy for pipeline steps.
	/// </summary>
	/// <param name="attempts">The default number of retry attempts.</param>
	/// <param name="backoffMs">The default backoff in milliseconds between retry attempts.</param>
	/// <returns>The current <see cref="FlowConfigurationBuilder"/> instance for method chaining.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown when <paramref name="attempts"/> is negative or <paramref name="backoffMs"/> is negative.
	/// </exception>
	public FlowConfigurationBuilder UseRetry( int attempts, int backoffMs = 100 ) {
		ArgumentOutOfRangeException.ThrowIfNegative( attempts );
		ArgumentOutOfRangeException.ThrowIfNegative( backoffMs );

		_configuration.DefaultRetryAttempts = attempts;
		_configuration.DefaultBackoffMs = backoffMs;

		return this;
	}

	/// <summary>
	/// Disables the default retry policy for pipeline steps.
	/// </summary>
	/// <returns>The current <see cref="FlowConfigurationBuilder"/> instance for method chaining.</returns>
	public FlowConfigurationBuilder DisableRetry( ) {
		_configuration.DefaultRetryAttempts = 0;

		return this;
	}

	/// <summary>
	/// Configures a custom <see cref="ActivitySource"/> for telemetry instrumentation.
	/// </summary>
	/// <param name="activitySource">The <see cref="ActivitySource"/> to use for telemetry.</param>
	/// <returns>The current <see cref="FlowConfigurationBuilder"/> instance for method chaining.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="activitySource"/> is null.</exception>
	public FlowConfigurationBuilder UseActivitySource( ActivitySource activitySource ) {
		ArgumentNullException.ThrowIfNull( activitySource );

		_configuration.ActivitySource = activitySource;

		return this;
	}

	/// <summary>
	/// Configures a custom <see cref="ActivitySource"/> with the specified name for telemetry instrumentation.
	/// </summary>
	/// <param name="name">The name of the <see cref="ActivitySource"/>.</param>
	/// <param name="version">The optional version of the <see cref="ActivitySource"/>.</param>
	/// <returns>The current <see cref="FlowConfigurationBuilder"/> instance for method chaining.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is empty or whitespace.</exception>
	public FlowConfigurationBuilder UseActivitySource( string name, string? version = null ) {
		ArgumentException.ThrowIfNullOrWhiteSpace( name );

		_configuration.ActivitySource = version != null
			? new ActivitySource( name, version )
			: new ActivitySource( name );

		return this;
	}

	/// <summary>
	/// Configures exception types that should be propagated without being handled by the pipeline.
	/// By default, all exceptions are handled internally and returned as failure results.
	/// Exception types specified here will be thrown and not caught by the pipeline execution.
	/// </summary>
	/// <param name="exceptionTypes">The exception types to propagate.</param>
	/// <returns>The current <see cref="FlowConfigurationBuilder"/> instance for method chaining.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="exceptionTypes"/> is null.</exception>
	/// <example>
	/// <code>
	/// builder.UseExceptionFilter( typeof( ArgumentException ), typeof( InvalidOperationException ) );
	/// </code>
	/// </example>
	public FlowConfigurationBuilder UseExceptionFilter( params Type[ ] exceptionTypes ) {
		ArgumentNullException.ThrowIfNull( exceptionTypes );

		foreach ( var exceptionType in exceptionTypes ) {
			if ( exceptionType != null && typeof( Exception ).IsAssignableFrom( exceptionType ) ) {
				_configuration.ExceptionTypesToPropagate.Add( exceptionType );
			}
		}

		return this;
	}

	/// <summary>
	/// Configures exception types that should be propagated without being handled by the pipeline.
	/// By default, all exceptions are handled internally and returned as failure results.
	/// Exception types specified here will be thrown and not caught by the pipeline execution.
	/// </summary>
	/// <typeparam name="TException">The exception type to propagate.</typeparam>
	/// <returns>The current <see cref="FlowConfigurationBuilder"/> instance for method chaining.</returns>
	/// <example>
	/// <code>
	/// builder.UseExceptionFilter&lt;ArgumentException&gt;( );
	/// </code>
	/// </example>
	public FlowConfigurationBuilder UseExceptionFilter<TException>( ) where TException : Exception {
		_configuration.ExceptionTypesToPropagate.Add( typeof( TException ) );

		return this;
	}

	/// <summary>
	/// Builds and returns the configured <see cref="PipelineConfiguration"/>.
	/// </summary>
	/// <returns>The configured <see cref="PipelineConfiguration"/> instance.</returns>
	public PipelineConfiguration Build( ) {
		return _configuration;
	}
}
