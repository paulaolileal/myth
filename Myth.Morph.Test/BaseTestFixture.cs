using Microsoft.Extensions.DependencyInjection;
using Myth.Extensions;
using Myth.Interfaces.Repositories.Results;
using Myth.Morph.Test.Service;
using Myth.Repositories.Results;
using Myth.ServiceProvider;

namespace Myth.Morph.Test;

/// <summary>
/// Base fixture for all Morph tests ensuring proper MythServiceProvider setup and cleanup
/// </summary>
[Collection( "Sequential" )]
[CollectionDefinition( "Sequential", DisableParallelization = true )]
public abstract class BaseTestFixture : IDisposable, IAsyncDisposable {

	/// <summary>
	/// Gets the current service provider from MythServiceProvider
	/// </summary>
	protected IServiceProvider ServiceProvider => MythServiceProvider.Current!;

	/// <summary>
	/// Initializes the test fixture with MythServiceProvider
	/// </summary>
	protected BaseTestFixture( ) {
		var services = new ServiceCollection( );
		services.AddLogging( );

		services.AddSingleton<IDescriptionResolver, DescriptionResolver>( );

		services.AddMorph( config => {
			config.AddGenericMorph( typeof( IPaginated<> ), typeof( Paginated<> ) );
		} );

		ConfigureServices( services );

		var serviceProvider = services.BuildServiceProvider( );

		MythServiceProvider.Initialize( serviceProvider );
	}

	/// <summary>
	/// Configure additional services for the test
	/// </summary>
	/// <param name="services">Service collection to configure</param>
	protected virtual void ConfigureServices( IServiceCollection services ) {
		// Override in derived classes to add test-specific services
	}

	/// <summary>
	/// Dispose resources synchronously
	/// </summary>
	public void Dispose( ) {
		Dispose( true );
		GC.SuppressFinalize( this );
	}

	/// <summary>
	/// Dispose resources asynchronously
	/// </summary>
	public async ValueTask DisposeAsync( ) {
		await DisposeAsyncCore( );
		Dispose( false );
		GC.SuppressFinalize( this );
	}

	/// <summary>
	/// Dispose implementation
	/// </summary>
	/// <param name="disposing">Whether disposing</param>
	protected virtual void Dispose( bool disposing ) {
		if ( disposing ) {
			MythServiceProvider.Reset( );
			( ServiceProvider as IDisposable )?.Dispose( );
		}
	}

	/// <summary>
	/// Async dispose implementation
	/// </summary>
	protected virtual async ValueTask DisposeAsyncCore( ) {
		MythServiceProvider.Reset( );
		if ( ServiceProvider is IAsyncDisposable asyncDisposable ) {
			await asyncDisposable.DisposeAsync( );
		} else {
			( ServiceProvider as IDisposable )?.Dispose( );
		}
	}
}