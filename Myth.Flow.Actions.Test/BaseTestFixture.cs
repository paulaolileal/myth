using Microsoft.Extensions.DependencyInjection;
using Myth.ServiceProvider;

namespace Myth.Flow.Actions.Test;

/// <summary>
/// Base test fixture that ensures proper MythServiceProvider initialization and cleanup
/// </summary>
[Collection( "Sequential" )]
[CollectionDefinition( "Sequential", DisableParallelization = true )]
public abstract class BaseTestFixture : IDisposable, IAsyncDisposable {
	protected IServiceProvider ServiceProvider => MythServiceProvider.Current!;

	protected BaseTestFixture( ) {
		var services = new ServiceCollection( );
		ConfigureServices( services );

		var serviceProvider = services.BuildServiceProvider( );
		MythServiceProvider.Initialize( serviceProvider );
	}

	/// <summary>
	/// Override this method to register services specific to the test
	/// </summary>
	/// <param name="services">The service collection to configure</param>
	protected abstract void ConfigureServices( IServiceCollection services );

	public void Dispose( ) {
		MythServiceProvider.Reset( );
		try {
			( ServiceProvider as IDisposable )?.Dispose( );
		} catch ( InvalidOperationException ex ) when ( ex.Message.Contains( "IAsyncDisposable" ) ) {
			// Some services only implement IAsyncDisposable
			// This is handled by IAsyncDisposable.DisposeAsync()
		}
	}

	public async ValueTask DisposeAsync( ) {
		MythServiceProvider.Reset( );
		if ( ServiceProvider is IAsyncDisposable asyncDisposable ) {
			await asyncDisposable.DisposeAsync( );
		} else {
			( ServiceProvider as IDisposable )?.Dispose( );
		}
	}
}
