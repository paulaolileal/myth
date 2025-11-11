using Myth.Builders;
using Myth.Models;

namespace Myth.Flow.Actions.Settings;

/// <summary>
/// Unified builder that combines Flow pipeline configuration with Flow.Actions CQRS configuration.
/// This builder inherits all Flow configuration methods and adds Flow.Actions specific configuration.
/// </summary>
public sealed class FlowWithActionsBuilder {
	private readonly FlowConfigurationBuilder _flowBuilder;
	private readonly FlowActionsBuilder _actionsBuilder;

	/// <summary>
	/// Initializes a new instance of the FlowWithActionsBuilder
	/// </summary>
	public FlowWithActionsBuilder( ) {
		_flowBuilder = new FlowConfigurationBuilder( );
		_actionsBuilder = new FlowActionsBuilder( );
	}

	// Flow configuration methods - delegate to FlowConfigurationBuilder

	/// <summary>
	/// Enables telemetry for pipeline execution
	/// </summary>
	/// <returns>The current builder instance for method chaining</returns>
	public FlowWithActionsBuilder UseTelemetry( ) {
		_flowBuilder.UseTelemetry( );
		return this;
	}

	/// <summary>
	/// Disables telemetry for pipeline execution
	/// </summary>
	/// <returns>The current builder instance for method chaining</returns>
	public FlowWithActionsBuilder DisableTelemetry( ) {
		_flowBuilder.DisableTelemetry( );
		return this;
	}

	/// <summary>
	/// Enables logging for pipeline execution
	/// </summary>
	/// <returns>The current builder instance for method chaining</returns>
	public FlowWithActionsBuilder UseLogging( ) {
		_flowBuilder.UseLogging( );
		return this;
	}

	/// <summary>
	/// Disables logging for pipeline execution
	/// </summary>
	/// <returns>The current builder instance for method chaining</returns>
	public FlowWithActionsBuilder DisableLogging( ) {
		_flowBuilder.DisableLogging( );
		return this;
	}

	/// <summary>
	/// Configures the default retry policy for pipeline steps
	/// </summary>
	/// <param name="attempts">The default number of retry attempts</param>
	/// <param name="backoffMs">The default backoff in milliseconds between retry attempts</param>
	/// <returns>The current builder instance for method chaining</returns>
	public FlowWithActionsBuilder UseRetry( int attempts, int backoffMs = 100 ) {
		_flowBuilder.UseRetry( attempts, backoffMs );
		return this;
	}

	/// <summary>
	/// Disables the default retry policy for pipeline steps
	/// </summary>
	/// <returns>The current builder instance for method chaining</returns>
	public FlowWithActionsBuilder DisableRetry( ) {
		_flowBuilder.DisableRetry( );
		return this;
	}

	/// <summary>
	/// Configures a custom ActivitySource for telemetry instrumentation
	/// </summary>
	/// <param name="activitySource">The ActivitySource to use for telemetry</param>
	/// <returns>The current builder instance for method chaining</returns>
	public FlowWithActionsBuilder UseActivitySource( System.Diagnostics.ActivitySource activitySource ) {
		_flowBuilder.UseActivitySource( activitySource );
		return this;
	}

	/// <summary>
	/// Configures a custom ActivitySource with the specified name for telemetry instrumentation
	/// </summary>
	/// <param name="name">The name of the ActivitySource</param>
	/// <param name="version">The optional version of the ActivitySource</param>
	/// <returns>The current builder instance for method chaining</returns>
	public FlowWithActionsBuilder UseActivitySource( string name, string? version = null ) {
		_flowBuilder.UseActivitySource( name, version );
		return this;
	}

	/// <summary>
	/// Configures exception types that should be propagated without being handled by the pipeline.
	/// By default, all exceptions are handled internally and returned as failure results.
	/// Exception types specified here will be thrown and not caught by the pipeline execution.
	/// </summary>
	/// <param name="exceptionTypes">The exception types to propagate</param>
	/// <returns>The current builder instance for method chaining</returns>
	/// <example>
	/// <code>
	/// builder.UseExceptionFilter( typeof( ArgumentException ), typeof( InvalidOperationException ) );
	/// </code>
	/// </example>
	public FlowWithActionsBuilder UseExceptionFilter( params Type[ ] exceptionTypes ) {
		_flowBuilder.UseExceptionFilter( exceptionTypes );
		return this;
	}

	/// <summary>
	/// Configures exception types that should be propagated without being handled by the pipeline.
	/// By default, all exceptions are handled internally and returned as failure results.
	/// Exception types specified here will be thrown and not caught by the pipeline execution.
	/// </summary>
	/// <typeparam name="TException">The exception type to propagate</typeparam>
	/// <returns>The current builder instance for method chaining</returns>
	/// <example>
	/// <code>
	/// builder.UseExceptionFilter&lt;ArgumentException&gt;( );
	/// </code>
	/// </example>
	public FlowWithActionsBuilder UseExceptionFilter<TException>( ) where TException : Exception {
		_flowBuilder.UseExceptionFilter<TException>( );
		return this;
	}

	// Flow.Actions configuration methods - delegate to FlowActionsBuilder

	/// <summary>
	/// Configures Flow.Actions features including CQRS, event handling, and message brokers
	/// </summary>
	/// <param name="configure">Configuration action for Flow.Actions specific settings</param>
	/// <returns>The current builder instance for method chaining</returns>
	public FlowWithActionsBuilder UseActions( Action<FlowActionsBuilder> configure ) {
		ArgumentNullException.ThrowIfNull( configure );
		configure( _actionsBuilder );
		return this;
	}

	// Internal methods to build configurations

	/// <summary>
	/// Builds the Flow pipeline configuration
	/// </summary>
	/// <returns>The configured PipelineConfiguration</returns>
	internal PipelineConfiguration BuildFlowConfiguration( ) {
		return _flowBuilder.Build( );
	}

	/// <summary>
	/// Builds the Flow.Actions configuration
	/// </summary>
	/// <returns>The configured FlowActionsConfiguration</returns>
	internal FlowActionsConfiguration BuildActionsConfiguration( ) {
		return _actionsBuilder.Build( );
	}
}
