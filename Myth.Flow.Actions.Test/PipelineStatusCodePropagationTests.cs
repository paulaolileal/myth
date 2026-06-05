using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Flow.Actions.Extensions;
using Myth.Flow.Actions.Settings;
using Myth.Flow.Actions.Test.Models;
using Myth.Interfaces;

namespace Myth.Flow.Actions.Test;

public class PipelineStatusCodePropagationTests : BaseTestFixture {

	protected override void ConfigureServices( IServiceCollection services ) {
		services.AddLogging( );
		services.AddFlow( config => config
			.UseActions( actions => {
				actions.UseInMemory( )
					   .UseCaching( cache => cache.ProviderType = CacheProviderType.Memory )
					   .ScanAssemblies( typeof( TestCommandHandler ).Assembly );
			} ) );
	}

	[Theory]
	[InlineData( "forbidden", HttpStatusCode.Forbidden )]
	[InlineData( "not-found", HttpStatusCode.NotFound )]
	[InlineData( "unauthorized", HttpStatusCode.Unauthorized )]
	[InlineData( "payment-required", HttpStatusCode.PaymentRequired )]
	[InlineData( "conflict", HttpStatusCode.Conflict )]
	[InlineData( "unprocessable-entity", HttpStatusCode.UnprocessableEntity )]
	public async Task Process_WhenCommandHandlerReturnsSemanticFailure_ShouldPreserveStatusCode(
		string failureMode,
		HttpStatusCode expectedStatusCode ) {
		var result = await Pipeline
			.Start( new FailingCommand { FailureMode = failureMode } )
			.Process<FailingCommand, string>( )
			.ExecuteAsync( );

		result.IsFailure.Should( ).BeTrue( );
		result.StatusCode.Should( ).Be( expectedStatusCode );
	}

	[Theory]
	[InlineData( "forbidden", HttpStatusCode.Forbidden )]
	[InlineData( "not-found", HttpStatusCode.NotFound )]
	[InlineData( "unauthorized", HttpStatusCode.Unauthorized )]
	public async Task Query_WhenQueryHandlerReturnsSemanticFailure_ShouldPreserveStatusCode(
		string failureMode,
		HttpStatusCode expectedStatusCode ) {
		var result = await PipelineExtensions
			.Start( new FailingQuery { FailureMode = failureMode } )
			.Query<FailingQuery, string>( )
			.ExecuteAsync( );

		result.IsFailure.Should( ).BeTrue( );
		result.StatusCode.Should( ).Be( expectedStatusCode );
	}

	[Fact]
	public async Task Process_WhenCommandSucceeds_ShouldHaveOkStatusCode( ) {
		var result = await Pipeline
			.Start( new TestCommand { Value = "ok" } )
			.Process<TestCommand, string>( )
			.ExecuteAsync( );

		result.IsSuccess.Should( ).BeTrue( );
		result.StatusCode.Should( ).Be( HttpStatusCode.OK );
	}

	[Fact]
	public async Task Process_WhenCommandReturnsNoContent_ShouldHaveNoContentStatusCode( ) {
		var result = await Pipeline
			.Start( new FailingCommand { FailureMode = "no-content" } )
			.Process<FailingCommand, string>( )
			.ExecuteAsync( );

		result.IsSuccess.Should( ).BeTrue( );
		result.StatusCode.Should( ).Be( HttpStatusCode.NoContent );
	}

	[Fact]
	public async Task Query_WhenQueryReturnsNoContent_ShouldHaveNoContentStatusCode( ) {
		var result = await PipelineExtensions
			.Start( new FailingQuery { FailureMode = "no-content" } )
			.Query<FailingQuery, string>( )
			.ExecuteAsync( );

		result.IsSuccess.Should( ).BeTrue( );
		result.StatusCode.Should( ).Be( HttpStatusCode.NoContent );
	}

	[Fact]
	public async Task Process_StatusCodeViaPipeline_ShouldMatchDispatcherDirect( ) {
		var dispatcher = ServiceProvider.GetRequiredService<IDispatcher>( );

		var dispatcherResult = await dispatcher.DispatchCommandAsync<FailingCommand, string>(
			new FailingCommand { FailureMode = "forbidden" } );

		var pipelineResult = await Pipeline
			.Start( new FailingCommand { FailureMode = "forbidden" } )
			.Process<FailingCommand, string>( )
			.ExecuteAsync( );

		dispatcherResult.IsFailure.Should( ).BeTrue( );
		pipelineResult.IsFailure.Should( ).BeTrue( );
		pipelineResult.StatusCode.Should( ).Be( dispatcherResult.StatusCode );
	}

	[Fact]
	public async Task Query_StatusCodeViaPipeline_ShouldMatchDispatcherDirect( ) {
		var dispatcher = ServiceProvider.GetRequiredService<IDispatcher>( );

		var dispatcherResult = await dispatcher.DispatchQueryAsync<FailingQuery, string>(
			new FailingQuery { FailureMode = "not-found" } );

		var pipelineResult = await PipelineExtensions
			.Start( new FailingQuery { FailureMode = "not-found" } )
			.Query<FailingQuery, string>( )
			.ExecuteAsync( );

		dispatcherResult.IsFailure.Should( ).BeTrue( );
		pipelineResult.IsFailure.Should( ).BeTrue( );
		pipelineResult.StatusCode.Should( ).Be( dispatcherResult.StatusCode );
	}

	[Fact]
	public async Task Process_SuccessStatusCodeViaPipeline_ShouldMatchDispatcherDirect( ) {
		var dispatcher = ServiceProvider.GetRequiredService<IDispatcher>( );

		var dispatcherResult = await dispatcher.DispatchCommandAsync<TestCommand, string>(
			new TestCommand { Value = "ok" } );

		var pipelineResult = await Pipeline
			.Start( new TestCommand { Value = "ok" } )
			.Process<TestCommand, string>( )
			.ExecuteAsync( );

		dispatcherResult.IsSuccess.Should( ).BeTrue( );
		pipelineResult.IsSuccess.Should( ).BeTrue( );
		pipelineResult.StatusCode.Should( ).Be( dispatcherResult.StatusCode );
	}
}
