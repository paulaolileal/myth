namespace Myth.Flow.Actions.Interfaces;

/// <summary>
/// Interface for empty pipeline builder that can be populated with Transform operations
/// </summary>
public interface IEmptyPipelineBuilder {

	/// <summary>
	/// Gets the service provider for dependency injection
	/// </summary>
	IServiceProvider ServiceProvider { get; }

	/// <summary>
	/// Transforms empty pipeline to a request type using a factory function
	/// </summary>
	/// <typeparam name="TRequest">The request type to create</typeparam>
	/// <param name="factory">Factory function to create the initial request</param>
	/// <returns>Action pipeline builder with the created request</returns>
	IActionPipelineBuilder<TRequest> Transform<TRequest>( Func<TRequest> factory );

	/// <summary>
	/// Transforms empty pipeline to a request type using an async factory function
	/// </summary>
	/// <typeparam name="TRequest">The request type to create</typeparam>
	/// <param name="factory">Async factory function to create the initial request</param>
	/// <returns>Action pipeline builder with the created request</returns>
	IActionPipelineBuilder<TRequest> TransformAsync<TRequest>( Func<Task<TRequest>> factory );
}