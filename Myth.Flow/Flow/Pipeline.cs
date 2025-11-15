using Myth.Builders;
using Myth.Interfaces;
using Myth.Models;
using Myth.ServiceProvider;

namespace Myth.Flow;

/// <summary>
/// Static entry point for creating pipelines.
/// Uses the centralized MythServiceProvider for all dependency resolution.
/// </summary>
public static class Pipeline {

	/// <summary>
	/// Starts a new pipeline with the specified input using the global MythServiceProvider.
	/// Uses <see cref="PipelineConfiguration"/> from the global provider if available.
	/// </summary>
	/// <typeparam name="TInput">Type of the pipeline input context.</typeparam>
	/// <param name="input">Initial input context for the pipeline.</param>
	/// <returns>An <see cref="IPipelineBuilder{TInput}"/> for building the pipeline.</returns>
	/// <exception cref="InvalidOperationException">Thrown when MythServiceProvider is not initialized</exception>
	public static IPipelineBuilder<TInput> Start<TInput>( TInput input ) {
		var serviceProvider = MythServiceProvider.GetRequired( );

		var config = serviceProvider.GetService( typeof( PipelineConfiguration ) ) as PipelineConfiguration
			?? new PipelineConfiguration( );

		return new PipelineBuilder<TInput>(
			input,
			serviceProvider,
			config );
	}

	/// <summary>
	/// Starts a new pipeline with the specified input and configuration action using the global MythServiceProvider.
	/// Uses <see cref="PipelineConfiguration"/> from the global provider if available, then applies the configuration action.
	/// </summary>
	/// <typeparam name="TInput">Type of the pipeline input context.</typeparam>
	/// <param name="input">Initial input context for the pipeline.</param>
	/// <param name="configure">Action to configure <see cref="PipelineConfiguration"/> before pipeline creation.</param>
	/// <returns>An <see cref="IPipelineBuilder{TInput}"/> for building the pipeline.</returns>
	/// <exception cref="InvalidOperationException">Thrown when MythServiceProvider is not initialized</exception>
	/// <exception cref="ArgumentNullException">Thrown when configure action is null</exception>
	public static IPipelineBuilder<TInput> Start<TInput>(
		TInput input,
		Action<PipelineConfiguration> configure ) {
		ArgumentNullException.ThrowIfNull( configure );

		var serviceProvider = MythServiceProvider.GetRequired( );
		var config = serviceProvider.GetService( typeof( PipelineConfiguration ) ) as PipelineConfiguration
			?? new PipelineConfiguration( );

		configure( config );

		return new PipelineBuilder<TInput>(
			input,
			serviceProvider,
			config );
	}
}
