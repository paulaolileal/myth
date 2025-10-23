using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Extensions;
using Myth.ServiceProvider;

namespace Myth.Commons.Test.ServiceProvider {

	/// <summary>
	/// Integration test that simulates the exact scenario reported by the user
	/// where Entity Framework repositories implementing only IAsyncDisposable
	/// caused disposal errors in ScopedService
	/// </summary>
	public class EntityFrameworkIntegrationTest {

		[Fact]
		public async Task ScopedService_WithEntityFrameworkLikeRepository_ShouldNotThrowAsyncDisposableError( ) {
			// Arrange - Simulate the user's exact scenario
			var services = new ServiceCollection( );
			services.AddScoped<IWeatherForecastRepository, WeatherForecastRepository>( );
			services.AddScopedServiceProvider( );

			var serviceProvider = services.BuildServiceProvider( );
			var scopedRepository = serviceProvider.GetRequiredService<IScopedService<IWeatherForecastRepository>>( );

			// Act & Assert - This should NOT throw the error:
			// 'WeatherForecastRepository' type only implements IAsyncDisposable. Use DisposeAsync to dispose the container.
			var result = await scopedRepository.ExecuteAsync( async repo => {
				return await repo.SearchPaginatedAsync( cancellationToken: CancellationToken.None );
			} );

			result.Should( ).NotBeNull( );
			result.Should( ).Be( "simulated-paginated-result" );
		}

		[Fact]
		public void SyncOperation_WithAsyncDisposableRepository_ShouldNotThrow( ) {
			// Arrange
			var services = new ServiceCollection( );
			services.AddScoped<IWeatherForecastRepository, WeatherForecastRepository>( );
			services.AddScopedServiceProvider( );

			var serviceProvider = services.BuildServiceProvider( );
			var scopedRepository = serviceProvider.GetRequiredService<IScopedService<IWeatherForecastRepository>>( );

			// Act & Assert - This should handle disposal correctly even in sync context
			var result = scopedRepository.Execute( repo => {
				return repo.GetName( );
			} );

			result.Should( ).Be( "WeatherForecastRepository" );
		}
	}

	// Simulating the user's repository structure
	public interface IWeatherForecastRepository {

		Task<string> SearchPaginatedAsync( CancellationToken cancellationToken = default );

		string GetName( );
	}

	/// <summary>
	/// Simulates a repository like ReadWriteRepositoryAsync that implements only IAsyncDisposable
	/// This is the exact pattern that was causing the original error
	/// </summary>
	public class WeatherForecastRepository : IWeatherForecastRepository, IAsyncDisposable {
		public bool IsDisposed { get; private set; }

		public Task<string> SearchPaginatedAsync( CancellationToken cancellationToken = default ) {
			if ( IsDisposed )
				throw new ObjectDisposedException( nameof( WeatherForecastRepository ) );
			return Task.FromResult( "simulated-paginated-result" );
		}

		public string GetName( ) {
			if ( IsDisposed )
				throw new ObjectDisposedException( nameof( WeatherForecastRepository ) );
			return nameof( WeatherForecastRepository );
		}

		public ValueTask DisposeAsync( ) {
			IsDisposed = true;
			return ValueTask.CompletedTask;
		}
	}
}