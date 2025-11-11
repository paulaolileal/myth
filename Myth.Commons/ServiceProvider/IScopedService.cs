namespace Myth.ServiceProvider;

/// <summary>
/// Provides a strongly-typed abstraction for executing operations within a service scope.
/// Automatically manages service scope lifecycle and resolves the specified service type
/// from the scoped service provider, regardless of the service's original registration lifetime.
/// </summary>
/// <typeparam name="T">The service type to resolve within the scope</typeparam>
/// <remarks>
/// <para>
/// This interface provides a clean, reusable pattern for working with scoped services
/// from transient contexts (like handlers, background services, or singleton services).
/// It automatically creates and disposes service scopes, ensuring proper dependency
/// injection lifecycle management.
/// </para>
/// <para>
/// The service type T will be resolved from each newly created scope, which means:
/// - Scoped services will use the scope-specific instance
/// - Transient services will get a new instance per operation
/// - Singleton services will use the application-wide singleton
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class OrderHandler : ICommandHandler&lt;CreateOrderCommand&gt; {
///     private readonly IScopedService&lt;IOrderRepository&gt; _repository;
///     private readonly IScopedService&lt;IEmailService&gt; _emailService;
///
///     public OrderHandler(
///         IScopedService&lt;IOrderRepository&gt; repository,
///         IScopedService&lt;IEmailService&gt; emailService) {
///         _repository = repository;
///         _emailService = emailService;
///     }
///
///     public async Task&lt;CommandResult&gt; HandleAsync(CreateOrderCommand command, CancellationToken ct) {
///         var order = await _repository.ExecuteAsync(repo =&gt; repo.CreateAsync(command.OrderData, ct));
///
///         await _emailService.ExecuteAsync(email =&gt;
///             email.SendOrderConfirmationAsync(order.Id, ct));
///
///         return CommandResult.Success();
///     }
/// }
/// </code>
/// </example>
public interface IScopedService<T> where T : class {

	/// <summary>
	/// Executes a synchronous operation within a new service scope.
	/// </summary>
	/// <typeparam name="TResult">The type of the operation result</typeparam>
	/// <param name="operation">The operation to execute with the scoped service instance</param>
	/// <returns>The result of the operation</returns>
	/// <exception cref="ArgumentNullException">Thrown when operation is null</exception>
	/// <exception cref="InvalidOperationException">Thrown when the service cannot be resolved</exception>
	/// <example>
	/// <code>
	/// var result = _scopedService.Execute(service =&gt; service.GetData());
	/// </code>
	/// </example>
	TResult Execute<TResult>( Func<T, TResult> operation );

	/// <summary>
	/// Executes an asynchronous operation within a new service scope.
	/// </summary>
	/// <typeparam name="TResult">The type of the operation result</typeparam>
	/// <param name="operation">The async operation to execute with the scoped service instance</param>
	/// <returns>A task containing the result of the operation</returns>
	/// <exception cref="ArgumentNullException">Thrown when operation is null</exception>
	/// <exception cref="InvalidOperationException">Thrown when the service cannot be resolved</exception>
	/// <example>
	/// <code>
	/// var result = await _scopedService.ExecuteAsync(service =&gt; service.GetDataAsync());
	/// </code>
	/// </example>
	Task<TResult> ExecuteAsync<TResult>( Func<T, Task<TResult>> operation );

	/// <summary>
	/// Executes a synchronous void operation within a new service scope.
	/// </summary>
	/// <param name="operation">The operation to execute with the scoped service instance</param>
	/// <exception cref="ArgumentNullException">Thrown when operation is null</exception>
	/// <exception cref="InvalidOperationException">Thrown when the service cannot be resolved</exception>
	/// <example>
	/// <code>
	/// _scopedService.Execute(service =&gt; service.ProcessData());
	/// </code>
	/// </example>
	void Execute( Action<T> operation );

	/// <summary>
	/// Executes an asynchronous void operation within a new service scope.
	/// </summary>
	/// <param name="operation">The async operation to execute with the scoped service instance</param>
	/// <returns>A task representing the async operation</returns>
	/// <exception cref="ArgumentNullException">Thrown when operation is null</exception>
	/// <exception cref="InvalidOperationException">Thrown when the service cannot be resolved</exception>
	/// <example>
	/// <code>
	/// await _scopedService.ExecuteAsync(service =&gt; service.ProcessDataAsync());
	/// </code>
	/// </example>
	Task ExecuteAsync( Func<T, Task> operation );
}
