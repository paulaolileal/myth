using Myth.Flow.Actions.Interfaces;
using Myth.Interfaces;
using Myth.Models;

namespace Myth.Flow.Actions;

/// <summary>
/// Internal implementation of action pipeline builder
/// </summary>
/// <typeparam name="TCurrent">The current request type in the pipeline</typeparam>
internal class ActionPipelineBuilder<TCurrent> : IActionPipelineBuilder<TCurrent> {
	private readonly IPipelineBuilder<ActionPipelineState<TCurrent>> _innerPipeline;

	/// <summary>
	/// Initializes a new instance of ActionPipelineBuilder
	/// </summary>
	/// <param name="innerPipeline">The underlying Myth.Flow pipeline</param>
	public ActionPipelineBuilder( IPipelineBuilder<ActionPipelineState<TCurrent>> innerPipeline ) {
		_innerPipeline = innerPipeline;
	}

	/// <summary>
	/// Gets the underlying Myth.Flow pipeline for extending functionality
	/// </summary>
	internal IPipelineBuilder<ActionPipelineState<TCurrent>> InnerPipeline => _innerPipeline;

	/// <summary>
	/// Executes the pipeline and returns the result
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Pipeline execution result</returns>
	public async Task<Result<TCurrent>> ExecuteAsync( CancellationToken cancellationToken = default ) {
		var result = await _innerPipeline.ExecuteAsync( cancellationToken );

		if ( result.IsFailure ) {
			return Result<TCurrent>.Failure( result.ErrorMessage!, result.Exception );
		}

		var finalRequest = result.Value!.CurrentRequest;
		if ( finalRequest == null ) {
			return Result<TCurrent>.Failure( "Pipeline completed without a current request" );
		}

		return Result<TCurrent>.Success( finalRequest );
	}
}
