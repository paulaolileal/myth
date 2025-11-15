using Myth.Flow.Actions.Extensions;
using Myth.Flow.Actions.Interfaces;

namespace Myth.Flow.Actions;

/// <summary>
/// Static Pipeline class for improved developer experience with Action-First API using MythServiceProvider
/// </summary>
public static class Pipeline {

	/// <summary>
	/// Starts a new action pipeline with an initial request using MythServiceProvider
	/// </summary>
	/// <typeparam name="TRequest">The initial request type</typeparam>
	/// <param name="request">The initial request to start the pipeline with</param>
	/// <returns>Action pipeline builder for method chaining</returns>
	/// <exception cref="InvalidOperationException">Thrown when MythServiceProvider is not initialized</exception>
	public static IActionPipelineBuilder<TRequest> Start<TRequest>( TRequest request ) {
		return PipelineExtensions.Start( request );
	}

	/// <summary>
	/// Starts an empty action pipeline using MythServiceProvider
	/// </summary>
	/// <returns>Empty pipeline builder that can be populated with Transform operations</returns>
	/// <exception cref="InvalidOperationException">Thrown when MythServiceProvider is not initialized</exception>
	public static IEmptyPipelineBuilder Start( ) {
		return PipelineExtensions.Start( );
	}
}
