using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Extensions;
using Myth.Flow.Actions.Extensions;
using Myth.Flow.Actions.Settings;
using Myth.Flow.Actions.Test.Models;
using Myth.Interfaces;
using Myth.Models;

namespace Myth.Flow.Actions.Test {

	public class EndToEndTests : BaseTestFixture {

		protected override void ConfigureServices( IServiceCollection services ) {
			services.AddLogging( );
			services.AddFlow( config => config
				.UseTelemetry( )
				.UseRetry( 3, 100 )
				.UseActions( actions => {
					actions.UseInMemory( )
						   .UseCaching( cache => {
							   cache.ProviderType = CacheProviderType.Memory;
							   cache.DefaultTtl = TimeSpan.FromMinutes( 10 );
						   } )
						   .ScanAssemblies( typeof( TestCommandHandler ).Assembly );
				} ) );
		}

		[Fact]
		public async Task CompleteFlow_ShouldExecuteAllSteps( ) {
			// Arrange
			var dispatcher = ServiceProvider.GetRequiredService<IDispatcher>( );

			// Act
			var commandResult = await dispatcher.DispatchCommandAsync<TestCommand, string>(
				new TestCommand { Value = "test-value" } );

			// Act
			var queryResult = await dispatcher.DispatchQueryAsync<TestQuery, string>(
				new TestQuery { Key = "test-key" } );

			// Act
			await dispatcher.PublishEventAsync( new TestEvent { Message = "test-message" } );

			// Assert
			commandResult.IsSuccess.Should( ).BeTrue( );
			commandResult.Data.Should( ).Contain( "test-value" );

			queryResult.IsSuccess.Should( ).BeTrue( );
			queryResult.Data.Should( ).Contain( "test-key" );
		}

		[Fact]
		public async Task PipelineIntegration_ShouldChainOperations( ) {
			// Arrange - using inherited service configuration

			// Act
			var result = await PipelineExtensions
				.Start( new TestCommand { Value = "initial-value" } )
				.Process<TestCommand, string>( )                                            // Command → string response
				.Transform( response => new TestQuery { Key = response } )              // Transform response → Query
				.Query<TestQuery, string>( )                                                // Execute query
				.Transform( queryResult => new TestEvent { Message = queryResult } )        // Transform → Event
				.Publish<TestEvent>( )                                                  // Publish event
				.ExecuteAsync( );

			// Assert
			result.IsSuccess.Should( ).BeTrue( );
			result.Value.Should( ).NotBeNull( );
		}

		[Fact]
		public async Task CachedQuery_ShouldReturnFromCacheOnSecondCall( ) {
			// Arrange
			var dispatcher = ServiceProvider.GetRequiredService<IDispatcher>( );

			var cacheOptions = new CacheOptions {
				Enabled = true,
				CacheKey = "test-cache-key",
				Ttl = TimeSpan.FromMinutes( 10 )
			};

			// Act - First call
			var firstResult = await dispatcher.DispatchQueryAsync<TestQuery, string>(
				new TestQuery { Key = "cached-test" },
				cacheOptions );

			// Act - Second call (should hit cache)
			var secondResult = await dispatcher.DispatchQueryAsync<TestQuery, string>(
				new TestQuery { Key = "cached-test" },
				cacheOptions );

			// Assert
			firstResult.IsSuccess.Should( ).BeTrue( );
			firstResult.FromCache.Should( ).BeFalse( );

			secondResult.IsSuccess.Should( ).BeTrue( );
			secondResult.FromCache.Should( ).BeTrue( );
			secondResult.Data.Should( ).Be( firstResult.Data );
		}

		[Fact]
		public async Task PipelineWithCache_ShouldUseCacheConfiguration( ) {
			// Arrange - using inherited service configuration

			// Act
			var result = await PipelineExtensions
				.Start( new TestQuery { Key = "cached-pipeline-test" } )
				.Query<TestQuery, string>( ( query, x ) => x.UseCache( "pipeline-cache-key", TimeSpan.FromMinutes( 5 ) ) )
				.ExecuteAsync( );

			// Assert
			result.IsSuccess.Should( ).BeTrue( );
			result.Value.Should( ).NotBeNull( );
		}
	}
}