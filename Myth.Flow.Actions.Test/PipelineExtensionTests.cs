using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Extensions;
using Myth.Flow.Actions.Extensions;
using Myth.Flow.Actions.Settings;
using Myth.Flow.Actions.Test.Models;

namespace Myth.Flow.Actions.Test;

public class PipelineExtensionsTests : BaseTestFixture {

	protected override void ConfigureServices( IServiceCollection services ) {
		services.AddLogging( );
		services.AddFlow( config => config
			.UseTelemetry( )
			.UseActions( actions => {
				actions.UseInMemory( )
					   .UseCaching( cache => cache.ProviderType = CacheProviderType.Memory )
					   .ScanAssemblies( typeof( TestCommandHandler ).Assembly );
			} ) );
	}

	[Fact]
	public async Task Process_WithValidCommand_ShouldExecuteSuccessfully( ) {
		// Act
		var result = await Pipeline
			.Start( new TestCommand { Value = "test" } )
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
			.Start( new TestQuery { Key = "query-key" } )
			.Query<TestQuery, string>( )
			.ExecuteAsync( );

		// Assert
		result.IsSuccess.Should( ).BeTrue( );
		result.Value.Should( ).Contain( "query-key" );
	}

	[Fact]
	public async Task Step_BeforeProcess_ShouldExecuteAndModifyState( ) {
		// Arrange
		string? stepExecuted = null;

		// Act
		var result = await Pipeline
			.Start( new TestCommand { Value = "original" } )
			.Step( state => {
				stepExecuted = "Step executed";
				// Create new command with modified value (since TestCommand is a record with init-only property)
				state.CurrentRequest = new TestCommand { Value = "modified" };
				return state;
			} )
			.Process<TestCommand, string>( )
			.ExecuteAsync( );

		// Assert
		result.IsSuccess.Should( ).BeTrue( );
		result.Value.Should( ).Be( "Handled: modified" );
		stepExecuted.Should( ).Be( "Step executed" );
	}

	[Fact]
	public async Task Tap_BeforeQuery_ShouldExecuteSideEffect( ) {
		// Arrange
		string? tapExecuted = null;

		// Act
		var result = await Pipeline
			.Start( new TestQuery { Key = "test-key" } )
			.Tap( state => {
				tapExecuted = $"Tap executed for: {state.CurrentRequest!.Key}";
			} )
			.Query<TestQuery, string>( )
			.ExecuteAsync( );

		// Assert
		result.IsSuccess.Should( ).BeTrue( );
		result.Value.Should( ).Contain( "test-key" );
		tapExecuted.Should( ).Be( "Tap executed for: test-key" );
	}

	[Fact]
	public async Task WithRetry_BeforeProcess_ShouldApplyRetryPolicy( ) {
		// Act
		var result = await Pipeline
			.Start( new TestCommand { Value = "retry-test" } )
			.WithRetry( maxAttempts: 3, backoffMs: 50 )
			.Process<TestCommand, string>( )
			.ExecuteAsync( );

		// Assert
		result.IsSuccess.Should( ).BeTrue( );
		result.Value.Should( ).Be( "Handled: retry-test" );
	}

	[Fact]
	public async Task WithTelemetry_BeforeQuery_ShouldEnableTelemetry( ) {
		// Act
		var result = await Pipeline
			.Start( new TestQuery { Key = "telemetry-test" } )
			.WithTelemetry( "QueryOperation" )
			.Query<TestQuery, string>( )
			.ExecuteAsync( );

		// Assert
		result.IsSuccess.Should( ).BeTrue( );
		result.Value.Should( ).Contain( "telemetry-test" );
	}

	[Fact]
	public async Task When_ConditionalExecution_ShouldExecuteWhenTrue( ) {
		// Act
		var result = await Pipeline
			.Start( new TestCommand { Value = "conditional" } )
			.When( state => state.CurrentRequest!.Value == "conditional", builder =>
				builder.Tap( state => state.CurrentRequest = new TestCommand { Value = "modified-by-condition" } ) )
			.Process<TestCommand, string>( )
			.ExecuteAsync( );

		// Assert
		result.IsSuccess.Should( ).BeTrue( );
		result.Value.Should( ).Be( "Handled: modified-by-condition" );
	}

	[Fact]
	public async Task When_ConditionalExecution_ShouldNotExecuteWhenFalse( ) {
		// Act
		var result = await Pipeline
			.Start( new TestCommand { Value = "original" } )
			.When( state => state.CurrentRequest!.Value == "different", builder =>
				builder.Tap( state => state.CurrentRequest = new TestCommand { Value = "should-not-change" } ) )
			.Process<TestCommand, string>( )
			.ExecuteAsync( );

		// Assert
		result.IsSuccess.Should( ).BeTrue( );
		result.Value.Should( ).Be( "Handled: original" );
	}

	[Fact]
	public async Task MultipleSteps_BeforeProcess_ShouldExecuteInOrder( ) {
		// Arrange
		var executionOrder = new List<string>( );

		// Act
		var result = await Pipeline
			.Start( new TestCommand { Value = "start" } )
			.Tap( state => executionOrder.Add( "Tap1" ) )
			.Step( state => {
				executionOrder.Add( "Step1" );
				state.CurrentRequest = new TestCommand { Value = state.CurrentRequest!.Value + "-step1" };
				return state;
			} )
			.TapAsync( async state => {
				await Task.Delay( 1 );
				executionOrder.Add( "TapAsync" );
			} )
			.Step( state => {
				executionOrder.Add( "Step2" );
				state.CurrentRequest = new TestCommand { Value = state.CurrentRequest!.Value + "-step2" };
				return state;
			} )
			.Process<TestCommand, string>( )
			.ExecuteAsync( );

		// Assert
		result.IsSuccess.Should( ).BeTrue( );
		result.Value.Should( ).Be( "Handled: start-step1-step2" );
		executionOrder.Should( ).Equal( "Tap1", "Step1", "TapAsync", "Step2" );
	}

	[Fact]
	public async Task Publish_WithValidEvent_ShouldNotThrow( ) {
		// Act
		var result = await PipelineExtensions
			.Start( new TestEvent { Message = "event" } )
			.Publish( )
			.ExecuteAsync( );

		// Assert
		result.IsSuccess.Should( ).BeTrue( );
	}

	[Fact]
	public async Task QueryCached_ShouldUseCacheKey( ) {
		// Act
		var result = await PipelineExtensions
			.Start( new TestQuery { Key = "cached-key" } )
			.Query<TestQuery, string>( ( query, x ) => x.UseCache( "test-cache-key", TimeSpan.FromMinutes( 5 ) ) )
			.ExecuteAsync( );

		// Assert
		result.IsSuccess.Should( ).BeTrue( );
	}

	[Fact]
	public async Task EmptyPipeline_WithTransform_ShouldWork( ) {
		// Act
		var result = await PipelineExtensions
			.Start( )
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
			.Start( new TestCommand { Value = "chain-test" } )
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
			.Start( new TestCommand { Value = "extensions" } )
			.Process<TestCommand, string>( )
			.ExecuteAsync( );

		var resultPipeline = await Pipeline
			.Start( new TestCommand { Value = "pipeline" } )
			.Process<TestCommand, string>( )
			.ExecuteAsync( );

		// Assert - Both should work identically
		resultExtensions.IsSuccess.Should( ).BeTrue( );
		resultExtensions.Value.Should( ).Be( "Handled: extensions" );

		resultPipeline.IsSuccess.Should( ).BeTrue( );
		resultPipeline.Value.Should( ).Be( "Handled: pipeline" );
	}
}
