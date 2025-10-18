using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Extensions;
using Myth.Flow.Actions.Extensions;
using Myth.Flow.Actions.Settings;
using Myth.Flow.Actions.Test.Models;
using Myth.Interfaces;
using Myth.Models;

namespace Myth.Flow.Actions.Test {

	public class EndToEndTests {

		[Fact]
		public async Task CompleteFlow_ShouldExecuteAllSteps( ) {
			// Arrange
			IServiceCollection services = new ServiceCollection( );
			services.AddLogging( );
			services.AddFlow( );
			services.AddFlowActions( options => {
				options.UseInMemory( )
					   .EnableCaching( cache => {
						   cache.ProviderType = CacheProviderType.Memory;
						   cache.DefaultTtl = TimeSpan.FromMinutes( 10 );
					   } )
					   .EnableRetry( retry => {
						   retry.MaxAttempts = 3;
						   retry.BackoffMs = 100;
					   } )
					   .ScanAssemblies( typeof( TestCommandHandler ).Assembly );
			} );

			var provider = services.BuildServiceProvider( );
			var dispatcher = provider.GetRequiredService<IDispatcher>( );

			// Act - Process Command
			var commandResult = await dispatcher.DispatchCommandAsync<TestCommand, string>(
				new TestCommand { Value = "test-value" } );

			// Act - Execute Query
			var queryResult = await dispatcher.DispatchQueryAsync<TestQuery, string>(
				new TestQuery { Key = "test-key" } );

			// Act - Publish Event
			await dispatcher.PublishEventAsync( new TestEvent { Message = "test-message" } );

			// Assert
			commandResult.IsSuccess.Should( ).BeTrue( );
			commandResult.Data.Should( ).Contain( "test-value" );

			queryResult.IsSuccess.Should( ).BeTrue( );
			queryResult.Data.Should( ).Contain( "test-key" );
		}

		[Fact]
		public async Task PipelineIntegration_ShouldChainOperations( ) {
			// Arrange
			var services = new ServiceCollection( );
			services.AddLogging( );
			services.AddFlow( );
			services.AddFlowActions( options => {
				options.UseInMemory( )
					   .EnableCaching( )
					   .ScanAssemblies( typeof( TestCommandHandler ).Assembly );
			} );

			var provider = services.BuildServiceProvider( );

			var context = new WorkflowContext {
				Input = "initial-value"
			};

			// Act
			var result = await Myth.Flow.Pipeline
				.Start( context, provider )
				.Process<WorkflowContext, TestCommand, string>(
					ctx => new TestCommand { Value = ctx.Input },
					( ctx, response ) => ctx.CommandResult = response )
				.Query<WorkflowContext, TestQuery, string>(
					ctx => new TestQuery { Key = ctx.Input },
					( ctx, response ) => ctx.QueryResult = response )
				.Publish<WorkflowContext, TestEvent>(
					ctx => new TestEvent { Message = ctx.Input } )
				.ExecuteAsync( );

			// Assert
			result.IsSuccess.Should( ).BeTrue( );
			result.Value!.CommandResult.Should( ).NotBeNull( );
			result.Value!.QueryResult.Should( ).NotBeNull( );
		}

		[Fact]
		public async Task CachedQuery_ShouldReturnFromCacheOnSecondCall( ) {
			// Arrange
			var services = new ServiceCollection( );
			services.AddLogging( );
			services.AddFlow( );
			services.AddFlowActions( options => {
				options.UseInMemory( )
					   .EnableCaching( cache => cache.ProviderType = CacheProviderType.Memory )
					   .ScanAssemblies( typeof( TestCommandHandler ).Assembly );
			} );

			var provider = services.BuildServiceProvider( );
			var dispatcher = provider.GetRequiredService<IDispatcher>( );

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

		private class WorkflowContext {
			public string Input { get; set; } = string.Empty;
			public string? CommandResult { get; set; }
			public string? QueryResult { get; set; }
		}
	}
}