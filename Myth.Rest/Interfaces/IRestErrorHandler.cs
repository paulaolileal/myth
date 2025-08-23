using Myth.Builders;
using Myth.Models;

namespace Myth.Interfaces;

/// <summary>
/// Interface for error post-processing configuration
/// </summary>
public interface IRestErrorHandler {

	/// <summary>
	/// Configure result handling after error configuration
	/// </summary>
	/// <param name="resultSettings">Result configuration action</param>
	/// <returns>Final result interface</returns>
	IRestResult OnResult( Action<ResultBuilder> resultSettings );

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