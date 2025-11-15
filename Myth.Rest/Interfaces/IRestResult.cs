using Myth.Models;

namespace Myth.Interfaces;

/// <summary>
/// Interface for final result building
/// </summary>
public interface IRestResult {

	/// <summary>
	/// Build and execute request
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>REST response</returns>
	Task<RestResponse> BuildAsync( CancellationToken cancellationToken = default );

	/// <summary>
	/// Build and execute request with typed response
	/// </summary>
	/// <typeparam name="TResult">Expected response type</typeparam>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>REST response</returns>
	Task<RestResponse> BuildAsync<TResult>( CancellationToken cancellationToken = default );
}
