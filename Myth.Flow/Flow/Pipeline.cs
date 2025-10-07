using Myth.Builders;
using Myth.Interfaces;
using Myth.Models;

namespace Myth.Flow {

	/// <summary>
	/// Static entry point for creating pipelines.
	/// Provides methods to start a pipeline with various configuration options.
	/// </summary>
	public static class Pipeline {
		private static IServiceProvider? _globalServiceProvider;

		/// <summary>
		/// Sets the global <see cref="IServiceProvider"/> used for pipeline creation.
		/// </summary>
		/// <param name="serviceProvider">The <see cref="IServiceProvider"/> to set globally.</param>
		internal static void SetGlobalServiceProvider( IServiceProvider serviceProvider ) {
			_globalServiceProvider = serviceProvider;
		}

		/// <summary>
		/// Starts a new pipeline with the specified input and the global <see cref="IServiceProvider"/>.
		/// Uses <see cref="PipelineConfiguration"/> from the global provider if available.
		/// </summary>
		/// <typeparam name="TInput">Type of the pipeline input context.</typeparam>
		/// <param name="input">Initial input context for the pipeline.</param>
		/// <returns>An <see cref="IPipelineBuilder{TInput}"/> for building the pipeline.</returns>
		public static IPipelineBuilder<TInput> Start<TInput>( TInput input ) {
			var config = _globalServiceProvider?.GetService( typeof( PipelineConfiguration ) ) as PipelineConfiguration
				?? new PipelineConfiguration( );

			return new PipelineBuilder<TInput>(
				input,
				_globalServiceProvider,
				config );
		}

		/// <summary>
		/// Starts a new pipeline with the specified input and service provider.
		/// Uses <see cref="PipelineConfiguration"/> from the provider if available.
		/// </summary>
		/// <typeparam name="TInput">Type of the pipeline input context.</typeparam>
		/// <param name="input">Initial input context for the pipeline.</param>
		/// <param name="serviceProvider">Optional <see cref="IServiceProvider"/> for dependency resolution.</param>
		/// <returns>An <see cref="IPipelineBuilder{TInput}"/> for building the pipeline.</returns>
		public static IPipelineBuilder<TInput> Start<TInput>(
			TInput input,
			IServiceProvider? serviceProvider ) {
			var provider = serviceProvider ?? _globalServiceProvider;
			var config = provider?.GetService( typeof( PipelineConfiguration ) ) as PipelineConfiguration
				?? new PipelineConfiguration( );

			return new PipelineBuilder<TInput>(
				input,
				provider,
				config );
		}

		/// <summary>
		/// Starts a new pipeline with the specified input, service provider, and configuration action.
		/// Uses <see cref="PipelineConfiguration"/> from the provider if available, then applies the configuration action.
		/// </summary>
		/// <typeparam name="TInput">Type of the pipeline input context.</typeparam>
		/// <param name="input">Initial input context for the pipeline.</param>
		/// <param name="serviceProvider">Optional <see cref="IServiceProvider"/> for dependency resolution.</param>
		/// <param name="configure">Action to configure <see cref="PipelineConfiguration"/> before pipeline creation.</param>
		/// <returns>An <see cref="IPipelineBuilder{TInput}"/> for building the pipeline.</returns>
		public static IPipelineBuilder<TInput> Start<TInput>(
			TInput input,
			IServiceProvider? serviceProvider,
			Action<PipelineConfiguration> configure ) {
			var provider = serviceProvider ?? _globalServiceProvider;
			var config = provider?.GetService( typeof( PipelineConfiguration ) ) as PipelineConfiguration
				?? new PipelineConfiguration( );

			configure( config );

			return new PipelineBuilder<TInput>(
				input,
				provider,
				config );
		}
	}
}