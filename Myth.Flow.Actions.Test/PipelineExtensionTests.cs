using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Extensions;
using Myth.Flow.Actions.Extensions;
using Myth.Flow.Actions.Settings;
using Myth.Flow.Actions.Test.Models;

namespace Myth.Flow.Actions.Test {

	public class PipelineExtensionsTests {
		private readonly IServiceProvider _serviceProvider;

		public PipelineExtensionsTests( ) {
			var services = new ServiceCollection( );
			services.AddLogging( );
			services.AddFlow( );
			services.AddFlowActions( options => {
				options.UseInMemory( )
					   .EnableCaching( cache => cache.ProviderType = CacheProviderType.Memory )
					   .ScanAssemblies( typeof( TestCommandHandler ).Assembly );
			} );

			_serviceProvider = services.BuildServiceProvider( );
		}

		[Fact]
		public async Task Process_WithValidCommand_ShouldExecuteSuccessfully( ) {
			// Act
			var result = await Pipeline
				.Start( new TestCommand { Value = "test" }, _serviceProvider )
				.Process<TestCommand, string>( )
				.ExecuteAsync( );

			// Assert
			result.IsSuccess.Should( ).BeTrue( );
			result.Value.Should( ).Be( "Handled: test" );
		}

		[Fact]
		public async Task Query_WithValidQuery_ShouldReturnResult( ) {
			// Act
			var result = await PipelineExtensions
				.Start( new TestQuery { Key = "query-key" }, _serviceProvider )
				.Query<TestQuery, string>( )
				.ExecuteAsync( );

			// Assert
			result.IsSuccess.Should( ).BeTrue( );
			result.Value.Should( ).Contain( "query-key" );
		}

		[Fact]
		public async Task Publish_WithValidEvent_ShouldNotThrow( ) {
			// Act
			var result = await PipelineExtensions
				.Start( new TestEvent { Message = "event" }, _serviceProvider )
				.Publish( )
				.ExecuteAsync( );

			// Assert
			result.IsSuccess.Should( ).BeTrue( );
		}

		[Fact]
		public async Task QueryCached_ShouldUseCacheKey( ) {
			// Act
			var result = await PipelineExtensions
				.Start( new TestQuery { Key = "cached-key" }, _serviceProvider )
				.Query<TestQuery, string>( x => x.UseCache( "test-cache-key", TimeSpan.FromMinutes( 5 ) ) )
				.ExecuteAsync( );

			// Assert
			result.IsSuccess.Should( ).BeTrue( );
		}

		[Fact]
		public async Task EmptyPipeline_WithTransform_ShouldWork( ) {
			// Act
			var result = await PipelineExtensions
				.Start( _serviceProvider )
				.Transform( ( ) => new TestCommand { Value = "from-empty-pipeline" } )
				.Process<TestCommand, string>( )
				.ExecuteAsync( );

			// Assert
			result.IsSuccess.Should( ).BeTrue( );
			result.Value.Should( ).Be( "Handled: from-empty-pipeline" );
		}

		[Fact]
		public async Task ChainedOperations_ShouldTransformCorrectly( ) {
			// Act
			var result = await PipelineExtensions
				.Start( new TestCommand { Value = "chain-test" }, _serviceProvider )
				.Process<TestCommand, string>( )                                            // Command → string
				.Transform( response => new TestQuery { Key = response } )              // string → Query
				.Query<TestQuery, string>( )                                                // Query → string
				.Transform( queryResult => new TestEvent { Message = queryResult } )        // string → Event
				.Publish<TestEvent>( )                                                  // Publish Event
				.ExecuteAsync( );

			// Assert
			result.IsSuccess.Should( ).BeTrue( );
			result.Value.Should( ).NotBeNull( );
		}

		[Fact]
		public async Task Pipeline_StaticClass_ShouldWork( ) {
			// Act - Test both APIs work the same
			var resultExtensions = await PipelineExtensions
				.Start( new TestCommand { Value = "extensions" }, _serviceProvider )
				.Process<TestCommand, string>( )
				.ExecuteAsync( );

			var resultPipeline = await Pipeline
				.Start( new TestCommand { Value = "pipeline" }, _serviceProvider )
				.Process<TestCommand, string>( )
				.ExecuteAsync( );

			// Assert - Both should work identically
			resultExtensions.IsSuccess.Should( ).BeTrue( );
			resultExtensions.Value.Should( ).Be( "Handled: extensions" );

			resultPipeline.IsSuccess.Should( ).BeTrue( );
			resultPipeline.Value.Should( ).Be( "Handled: pipeline" );
		}
	}
}