using Microsoft.Extensions.DependencyInjection;

namespace Myth.ServiceProvider;

/// <summary>
/// Default implementation of IScopedService that manages service scope lifecycle
/// and automatically resolves services from the scoped service provider.
/// </summary>
/// <typeparam name="T">The service type to resolve within the scope</typeparam>
internal sealed class ScopedService<T> : IScopedService<T> where T : class {
	private readonly IServiceScopeFactory _scopeFactory;

	/// <summary>
	/// Initializes a new instance of the ScopedService class.
	/// </summary>
	/// <param name="scopeFactory">The service scope factory used to create scopes</param>
	/// <exception cref="ArgumentNullException">Thrown when scopeFactory is null</exception>
	public ScopedService( IServiceScopeFactory scopeFactory ) {
		ArgumentNullException.ThrowIfNull( scopeFactory );
		_scopeFactory = scopeFactory;
	}

	/// <inheritdoc />
	public TResult Execute<TResult>( Func<T, TResult> operation ) {
		ArgumentNullException.ThrowIfNull( operation );

		using var scope = _scopeFactory.CreateScope( );
		var service = ResolveService( scope.ServiceProvider );

		return operation( service );
	}

	/// <inheritdoc />
	public async Task<TResult> ExecuteAsync<TResult>( Func<T, Task<TResult>> operation ) {
		ArgumentNullException.ThrowIfNull( operation );

		using var scope = _scopeFactory.CreateScope( );
		var service = ResolveService( scope.ServiceProvider );

		return await operation( service );
	}

	/// <inheritdoc />
	public void Execute( Action<T> operation ) {
		ArgumentNullException.ThrowIfNull( operation );

		using var scope = _scopeFactory.CreateScope( );
		var service = ResolveService( scope.ServiceProvider );

		operation( service );
	}

	/// <inheritdoc />
	public async Task ExecuteAsync( Func<T, Task> operation ) {
		ArgumentNullException.ThrowIfNull( operation );

		using var scope = _scopeFactory.CreateScope( );
		var service = ResolveService( scope.ServiceProvider );

		await operation( service );
	}

	/// <summary>
	/// Resolves the service of type T from the provided service provider.
	/// </summary>
	/// <param name="serviceProvider">The service provider to resolve from</param>
	/// <returns>The resolved service instance</returns>
	/// <exception cref="InvalidOperationException">Thrown when the service cannot be resolved</exception>
	private static T ResolveService( IServiceProvider serviceProvider ) {
		var service = serviceProvider.GetService<T>( );

		if ( service == null ) {
			throw new InvalidOperationException(
				$"Service of type '{typeof( T ).FullName}' could not be resolved. " +
				$"Ensure the service is registered in the dependency injection container." );
		}

		return service;
	}
}