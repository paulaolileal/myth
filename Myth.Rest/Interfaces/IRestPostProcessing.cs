using Myth.Builders;
using Myth.Models;

namespace Myth.Interfaces;

/// <summary>
/// Interface for post-processing configuration
/// </summary>
public interface IRestPostProcessing {

	/// <summary>
	/// Configure error handling
	/// </summary>
	/// <param name="errorSettings">Error configuration action</param>
	/// <returns>Error post-processing interface</returns>
	IRestErrorHandler OnError( Action<ErrorBuilder> errorSettings );

	/// <summary>
	/// Configure result handling
	/// </summary>
	/// <param name="resultSettings">Result configuration action</param>
	/// <returns>Result post-processing interface</returns>
	IRestResultHandler OnResult( Action<ResultBuilder> resultSettings );

	/// <summary>
	/// Build and execute request directly
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>REST response</returns>
	Task<RestResponse> BuildAsync( CancellationToken cancellationToken = default );

	/// <summary>
	/// Build and execute request with typed response directly
	/// </summary>
	/// <typeparam name="TResult">Expected response type</typeparam>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>REST response</returns>
	Task<RestResponse> BuildAsync<TResult>( CancellationToken cancellationToken = default );
}