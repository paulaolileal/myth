using Myth.Builders;
using Myth.Models;

namespace Myth.Interfaces;

/// <summary>
/// Interface for result post-processing configuration
/// </summary>
public interface IRestResultHandler {

	/// <summary>
	/// Configure error handling after result configuration
	/// </summary>
	/// <param name="errorSettings">Error configuration action</param>
	/// <returns>Final result interface</returns>
	IRestResult OnError( Action<ErrorBuilder> errorSettings );

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