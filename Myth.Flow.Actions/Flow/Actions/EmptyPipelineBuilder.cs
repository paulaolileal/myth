using Myth.Flow.Actions.Interfaces;
using Myth.ServiceProvider;

namespace Myth.Flow.Actions;

/// <summary>
/// Internal implementation of empty pipeline builder that uses MythServiceProvider
/// </summary>
internal class EmptyPipelineBuilder : IEmptyPipelineBuilder {

	/// <summary>
	/// Gets the service provider for dependency injection
	/// </summary>
	public IServiceProvider ServiceProvider { get; }

	/// <summary>
	/// Initializes a new instance of EmptyPipelineBuilder using MythServiceProvider
	/// </summary>
	/// <exception cref="InvalidOperationException">Thrown when MythServiceProvider is not initialized</exception>
	public EmptyPipelineBuilder( ) {
		ServiceProvider = MythServiceProvider.GetRequired( );
	}

	/// <summary>
	/// Transforms empty pipeline to a request type using a factory function
	/// </summary>
	/// <typeparam name="TRequest">The request type to create</typeparam>
	/// <param name="factory">Factory function to create the initial request</param>
	/// <returns>Action pipeline builder with the created request</returns>
	public IActionPipelineBuilder<TRequest> Transform<TRequest>( Func<TRequest> factory ) {
		var request = factory( );
		var state = new ActionPipelineState<TRequest>( request );
		var pipeline = Myth.Flow.Pipeline.Start( state );
		return new ActionPipelineBuilder<TRequest>( pipeline );
	}

	/// <summary>
	/// Transforms empty pipeline to a request type using an async factory function
	/// </summary>
	/// <typeparam name="TRequest">The request type to create</typeparam>
	/// <param name="factory">Async factory function to create the initial request</param>
	/// <returns>Action pipeline builder with the created request</returns>
	public IActionPipelineBuilder<TRequest> TransformAsync<TRequest>( Func<Task<TRequest>> factory ) {
		var state = new ActionPipelineState<TRequest>( default( TRequest )! );
		var pipeline = Myth.Flow.Pipeline.Start( state )
			.StepAsync( async s => {
				var request = await factory( );
				s.CurrentRequest = request;
				return s;
			} );
		return new ActionPipelineBuilder<TRequest>( pipeline );
	}
}