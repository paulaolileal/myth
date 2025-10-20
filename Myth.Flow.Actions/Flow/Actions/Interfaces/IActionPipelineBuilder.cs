using Myth.Models;

namespace Myth.Flow.Actions.Interfaces;

/// <summary>
/// Pipeline builder for Action-First pattern without context boilerplate
/// </summary>
/// <typeparam name="TCurrent">The current request type in the pipeline</typeparam>
public interface IActionPipelineBuilder<TCurrent> {

	/// <summary>
	/// Executes the pipeline and returns the result
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Pipeline execution result</returns>
	Task<Result<TCurrent>> ExecuteAsync( CancellationToken cancellationToken = default );
}
