using Myth.Flow.Actions.Extensions;
using Myth.Flow.Actions.Interfaces;

namespace Myth.Flow.Actions;

/// <summary>
/// Static Pipeline class for improved developer experience with Action-First API
/// </summary>
public static class Pipeline {

	/// <summary>
	/// Starts a new action pipeline with an initial request
	/// </summary>
	/// <typeparam name="TRequest">The initial request type</typeparam>
	/// <param name="request">The initial request to start the pipeline with</param>
	/// <param name="serviceProvider">Service provider for dependency injection</param>
	/// <returns>Action pipeline builder for method chaining</returns>
	public static IActionPipelineBuilder<TRequest> Start<TRequest>( TRequest request, IServiceProvider serviceProvider ) {
		return PipelineExtensions.Start( request, serviceProvider );
	}

	/// <summary>
	/// Starts a new action pipeline with an initial request using default service provider
	/// </summary>
	/// <typeparam name="TRequest">The initial request type</typeparam>
	/// <param name="request">The initial request to start the pipeline with</param>
	/// <returns>Action pipeline builder for method chaining</returns>
	public static IActionPipelineBuilder<TRequest> Start<TRequest>( TRequest request ) {
		return PipelineExtensions.Start( request );
	}

	/// <summary>
	/// Starts an empty action pipeline for functional/utility scenarios
	/// </summary>
	/// <param name="serviceProvider">Service provider for dependency injection</param>
	/// <returns>Empty pipeline builder that can be populated with Transform operations</returns>
	public static IEmptyPipelineBuilder Start( IServiceProvider serviceProvider ) {
		return PipelineExtensions.Start( serviceProvider );
	}

	/// <summary>
	/// Starts an empty action pipeline with default service provider
	/// </summary>
	/// <returns>Empty pipeline builder that can be populated with Transform operations</returns>
	public static IEmptyPipelineBuilder Start( ) {
		return PipelineExtensions.Start( );
	}
}