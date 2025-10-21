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
	/// Initializes a new instance of ActionPipelineState
	/// </summary>
	public ActionPipelineState( ) { }

	/// <summary>
	/// Initializes a new instance of ActionPipelineState with a request
	/// </summary>
	/// <param name="request">The initial request</param>
	/// <param name="serviceProvider">The service provider</param>
	public ActionPipelineState( TCurrent request, IServiceProvider serviceProvider ) {
		CurrentRequest = request;
		ServiceProvider = serviceProvider;
		CorrelationId = Guid.NewGuid( ).ToString( );
	}

	/// <summary>
	/// Initializes a new instance of ActionPipelineState without a request
	/// </summary>
	/// <param name="serviceProvider">The service provider</param>
	public ActionPipelineState( IServiceProvider serviceProvider ) {
		ServiceProvider = serviceProvider;
		CorrelationId = Guid.NewGuid( ).ToString( );
	}
}