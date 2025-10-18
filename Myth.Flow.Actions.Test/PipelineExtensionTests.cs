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
			// Arrange
			var context = new TestContext { Value = "test" };

			// Act
			var result = await Myth.Flow.Pipeline
				.Start( context, _serviceProvider )
				.Process<TestContext, TestCommand, string>(
					ctx => new TestCommand { Value = ctx.Value },
					( ctx, response ) => ctx.Result = response )
				.ExecuteAsync( );

			// Assert
			result.IsSuccess.Should( ).BeTrue( );
			result.Value!.Result.Should( ).Be( "Handled: test" );
		}

		[Fact]
		public async Task Query_WithValidQuery_ShouldReturnResult( ) {
			// Arrange
			var context = new TestContext { Value = "query-key" };

			// Act
			var result = await Myth.Flow.Pipeline
				.Start( context, _serviceProvider )
				.Query<TestContext, TestQuery, string>(
					ctx => new TestQuery { Key = ctx.Value },
					( ctx, response ) => ctx.Result = response )
				.ExecuteAsync( );

			// Assert
			result.IsSuccess.Should( ).BeTrue( );
			result.Value!.Result.Should( ).Contain( "query-key" );
		}

		[Fact]
		public async Task Publish_WithValidEvent_ShouldNotThrow( ) {
			// Arrange
			var context = new TestContext { Value = "event" };

			// Act
			var result = await Myth.Flow.Pipeline
				.Start( context, _serviceProvider )
				.Publish<TestContext, TestEvent>(
					ctx => new TestEvent { Message = ctx.Value } )
				.ExecuteAsync( );

			// Assert
			result.IsSuccess.Should( ).BeTrue( );
		}

		[Fact]
		public async Task QueryCached_ShouldUseCacheKey( ) {
			// Arrange
			var context = new TestContext { Value = "cached-key" };

			// Act
			var result = await Pipeline
				.Start( context, _serviceProvider )
				.Query<TestContext, TestQuery, string>(
					ctx => new TestQuery { Key = ctx.Value },
					( ctx, response ) => ctx.Result = response,
					cacheKey: "test-cache-key",
					ttl: TimeSpan.FromMinutes( 5 ) )
				.ExecuteAsync( );

			// Assert
			result.IsSuccess.Should( ).BeTrue( );
		}

		private class TestContext {
			public string Value { get; set; } = string.Empty;
			public string? Result { get; set; }
		}
	}
}