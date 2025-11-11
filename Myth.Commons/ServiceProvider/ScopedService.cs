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

		var scope = _scopeFactory.CreateScope( );
		try {
			var service = ResolveService( scope.ServiceProvider );
			return operation( service );
		} finally {
			DisposeScope( scope );
		}
	}

	/// <inheritdoc />
	public async Task<TResult> ExecuteAsync<TResult>( Func<T, Task<TResult>> operation ) {
		ArgumentNullException.ThrowIfNull( operation );

		var scope = _scopeFactory.CreateScope( );
		try {
			var service = ResolveService( scope.ServiceProvider );
			return await operation( service );
		} finally {
			await DisposeScopeAsync( scope );
		}
	}

	/// <inheritdoc />
	public void Execute( Action<T> operation ) {
		ArgumentNullException.ThrowIfNull( operation );

		var scope = _scopeFactory.CreateScope( );
		try {
			var service = ResolveService( scope.ServiceProvider );
			operation( service );
		} finally {
			DisposeScope( scope );
		}
	}

	/// <inheritdoc />
	public async Task ExecuteAsync( Func<T, Task> operation ) {
		ArgumentNullException.ThrowIfNull( operation );

		var scope = _scopeFactory.CreateScope( );
		try {
			var service = ResolveService( scope.ServiceProvider );
			await operation( service );
		} finally {
			await DisposeScopeAsync( scope );
		}
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

	/// <summary>
	/// Disposes a service scope using the appropriate disposal method.
	/// Attempts async disposal first (IAsyncDisposable), then falls back to sync disposal (IDisposable).
	/// </summary>
	/// <param name="scope">The service scope to dispose</param>
	private static void DisposeScope( IServiceScope scope ) {
		// Try async disposal first, but execute synchronously
		if ( scope is IAsyncDisposable asyncDisposable ) {
			try {
				asyncDisposable.DisposeAsync( ).AsTask( ).GetAwaiter( ).GetResult( );
				return;
			} catch {
				// If async disposal fails, fall back to sync disposal
			}
		}

		// Fall back to sync disposal
		scope.Dispose( );
	}

	/// <summary>
	/// Asynchronously disposes a service scope using the appropriate disposal method.
	/// Attempts async disposal first (IAsyncDisposable), then falls back to sync disposal (IDisposable).
	/// </summary>
	/// <param name="scope">The service scope to dispose</param>
	/// <returns>A task representing the disposal operation</returns>
	private static async ValueTask DisposeScopeAsync( IServiceScope scope ) {
		// Try async disposal first
		if ( scope is IAsyncDisposable asyncDisposable ) {
			await asyncDisposable.DisposeAsync( );
			return;
		}

		// Fall back to sync disposal
		scope.Dispose( );
	}
}
