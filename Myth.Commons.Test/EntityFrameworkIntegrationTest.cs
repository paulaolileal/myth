using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.ServiceProvider;

namespace Myth.Commons.Test {

	/// <summary>
	/// Integration test that simulates the exact scenario reported by the user
	/// where Entity Framework repositories implementing only IAsyncDisposable
	/// caused disposal errors in ScopedService
	/// </summary>
	public class EntityFrameworkIntegrationTest : BaseTestFixture {

		protected override void ConfigureServices( IServiceCollection services ) {
			services.AddScoped<IWeatherForecastRepository, WeatherForecastRepository>( );
		}

		[Fact]
		public async Task ScopedService_WithEntityFrameworkLikeRepository_ShouldNotThrowAsyncDisposableError( ) {
			// Arrange 
			var scopedRepository = ServiceProvider.GetRequiredService<IScopedService<IWeatherForecastRepository>>( );

			// Act & Assert
			var result = await scopedRepository.ExecuteAsync( async repo => {
				return await repo.SearchPaginatedAsync( cancellationToken: CancellationToken.None );
			} );

			result.Should( ).NotBeNull( );
			result.Should( ).Be( "simulated-paginated-result" );
		}

		[Fact]
		public void SyncOperation_WithAsyncDisposableRepository_ShouldNotThrow( ) {
			// Arrange
			var scopedRepository = ServiceProvider.GetRequiredService<IScopedService<IWeatherForecastRepository>>( );

			// Act & Assert
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
			return IsDisposed
				? throw new ObjectDisposedException( nameof( WeatherForecastRepository ) )
				: Task.FromResult( "simulated-paginated-result" );
		}

		public string GetName( ) {
			return IsDisposed
				? throw new ObjectDisposedException( nameof( WeatherForecastRepository ) ) 
				:  nameof( WeatherForecastRepository );
		}

		public ValueTask DisposeAsync( ) {
			IsDisposed = true;
			return ValueTask.CompletedTask;
		}
	}
}