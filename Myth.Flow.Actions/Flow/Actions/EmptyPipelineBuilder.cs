using Myth.Flow.Actions.Interfaces;

namespace Myth.Flow.Actions;

/// <summary>
/// Internal implementation of empty pipeline builder
/// </summary>
internal class EmptyPipelineBuilder : IEmptyPipelineBuilder {

	/// <summary>
	/// Gets the service provider for dependency injection
	/// </summary>
	public IServiceProvider ServiceProvider { get; }

	/// <summary>
	/// Initializes a new instance of EmptyPipelineBuilder
	/// </summary>
	/// <param name="serviceProvider">Service provider for dependency injection</param>
	public EmptyPipelineBuilder( IServiceProvider serviceProvider ) {
		ServiceProvider = serviceProvider;
	}

	/// <summary>
	/// Transforms empty pipeline to a request type using a factory function
	/// </summary>
	/// <typeparam name="TRequest">The request type to create</typeparam>
	/// <param name="factory">Factory function to create the initial request</param>
	/// <returns>Action pipeline builder with the created request</returns>
	public IActionPipelineBuilder<TRequest> Transform<TRequest>( Func<TRequest> factory ) {
		var request = factory( );
		var state = new ActionPipelineState<TRequest>( request, ServiceProvider );
		var pipeline = Myth.Flow.Pipeline.Start( state, ServiceProvider );
		return new ActionPipelineBuilder<TRequest>( pipeline );
	}

	/// <summary>
	/// Transforms empty pipeline to a request type using an async factory function
	/// </summary>
	/// <typeparam name="TRequest">The request type to create</typeparam>
	/// <param name="factory">Async factory function to create the initial request</param>
	/// <returns>Action pipeline builder with the created request</returns>
	public IActionPipelineBuilder<TRequest> TransformAsync<TRequest>( Func<Task<TRequest>> factory ) {
		var state = new ActionPipelineState<TRequest>( default!, ServiceProvider );
		var pipeline = Myth.Flow.Pipeline.Start( state, ServiceProvider )
			.StepAsync<IServiceProvider>( async ( _, s ) => {
				var request = await factory( );
				s.CurrentRequest = request;
				return s;
			} );
		return new ActionPipelineBuilder<TRequest>( pipeline );
	}
}