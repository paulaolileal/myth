using Myth.ServiceProvider;

namespace Myth.Flow.Actions;

/// <summary>
/// State container for action pipelines
/// </summary>
/// <typeparam name="TCurrent">The current request type</typeparam>
public class ActionPipelineState<TCurrent> {

	/// <summary>
	/// Gets or sets the current request in the pipeline
	/// </summary>
	public TCurrent? CurrentRequest { get; set; }

	/// <summary>
	/// Gets or sets the last operation result
	/// </summary>
	public object? LastResult { get; set; }

	/// <summary>
	/// Gets or sets the service provider for dependency injection
	/// </summary>
	public IServiceProvider ServiceProvider { get; set; } = null!;

	/// <summary>
	/// Gets or sets the correlation ID for distributed tracing
	/// </summary>
	public string? CorrelationId { get; set; }

	/// <summary>
	/// Initializes a new instance of ActionPipelineState using MythServiceProvider
	/// </summary>
	/// <exception cref="InvalidOperationException">Thrown when MythServiceProvider is not initialized</exception>
	public ActionPipelineState( ) {
		ServiceProvider = MythServiceProvider.GetRequired( );
		CorrelationId = Guid.NewGuid( ).ToString( );
	}

	/// <summary>
	/// Initializes a new instance of ActionPipelineState with a request using MythServiceProvider
	/// </summary>
	/// <param name="request">The initial request</param>
	/// <exception cref="InvalidOperationException">Thrown when MythServiceProvider is not initialized</exception>
	public ActionPipelineState( TCurrent request ) {
		CurrentRequest = request;
		ServiceProvider = MythServiceProvider.GetRequired( );
		CorrelationId = Guid.NewGuid( ).ToString( );
	}
}