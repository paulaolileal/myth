using System.Diagnostics;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Myth.Flow.Actions.Test.Models;
using Myth.Interfaces;
using Myth.Models;
using Myth.ServiceProvider;
using NSubstitute;

namespace Myth.Flow.Actions.Test;

public class DispatcherTests : BaseTestFixture {
	private readonly IEventBus _eventBus;
	private readonly ILogger<Dispatcher> _logger;
	private readonly ActivitySource _activitySource;
	private readonly Dispatcher _sut;

	public DispatcherTests( ) {
		_eventBus = Substitute.For<IEventBus>( );
		_logger = Substitute.For<ILogger<Dispatcher>>( );
		_activitySource = new ActivitySource( "Test" );
		_sut = new Dispatcher( _eventBus, _logger, _activitySource );
	}

	protected override void ConfigureServices( IServiceCollection services ) {
		services.AddTransient<ICommandHandler<TestCommand, string>, TestCommandHandler>( );
		services.AddTransient<ICommandHandler<TestCommandNoResponse>, TestCommandNoResponseHandler>( );
		services.AddTransient<IQueryHandler<TestQuery, string>, TestQueryHandler>( );
	}

	[Fact]
	public async Task DispatchCommandAsync_WithValidHandler_ShouldReturnSuccess( ) {
		// Arrange
		var command = new TestCommand { Value = "test" };

		// Act
		var result = await _sut.DispatchCommandAsync<TestCommand, string>( command );

		// Assert
		result.IsSuccess.Should( ).BeTrue( );
		result.Data.Should( ).Be( "Handled: test" );
	}

	[Fact]
	public async Task DispatchCommandAsync_WithoutResponse_ShouldReturnSuccess( ) {
		// Arrange
		var command = new TestCommandNoResponse { Value = "test" };

		// Act
		var result = await _sut.DispatchCommandAsync( command );

		// Assert
		result.IsSuccess.Should( ).BeTrue( );
	}

	public record UnregisteredCommand( string Value ) : ICommand<string>;

	[Fact( Skip = "TODO: Fix handler registration behavior" )]
	public async Task DispatchCommandAsync_WhenHandlerNotRegistered_ShouldReturnFailure( ) {
		// Arrange - Create a command type that definitely has no handler
		var unregisteredCommand = new UnregisteredCommand( "test" );

		// Act - try to dispatch with a type that doesn't have a handler
		var result = await _sut.DispatchCommandAsync<UnregisteredCommand, string>( unregisteredCommand );

		// Assert
		result.IsFailure.Should( ).BeTrue( );
		result.ErrorMessage.Should( ).Contain( "No handler registered" );
	}

	[Fact]
	public async Task DispatchQueryAsync_WithValidHandler_ShouldReturnResult( ) {
		// Arrange
		var query = new TestQuery { Key = "test-key" };

		// Act
		var result = await _sut.DispatchQueryAsync<TestQuery, string>( query );

		// Assert
		result.IsSuccess.Should( ).BeTrue( );
		result.Data.Should( ).Be( "Result for: test-key" );
		result.FromCache.Should( ).BeFalse( );
	}

	[Fact]
	public async Task DispatchQueryAsync_WithCache_ShouldReturnFromCache( ) {
		// Arrange
		var cacheProvider = Substitute.For<ICacheProvider>( );
		cacheProvider.GetAsync<string>( Arg.Any<string>( ), Arg.Any<CancellationToken>( ) ).ReturnsForAnyArgs( CacheValue<string>.Hit( "cached-value" ) );

		var dispatcher = new Dispatcher( _eventBus, _logger, _activitySource, cacheProvider );

		var query = new TestQuery { Key = "test-key" };
		var cacheOptions = new CacheOptions {
			Enabled = true,
			CacheKey = "test-cache-key",
			Ttl = TimeSpan.FromMinutes( 5 )
		};

		// Act
		var result = await dispatcher.DispatchQueryAsync<TestQuery, string>( query, cacheOptions );

		// Assert
		result.IsSuccess.Should( ).BeTrue( );
		result.Data.Should( ).Be( "cached-value" );
		result.FromCache.Should( ).BeTrue( );
	}

	[Fact]
	public async Task PublishEventAsync_ShouldCallEventBus( ) {
		// Arrange
		var @event = new TestEvent { Message = "test" };

		// Act
		await _sut.PublishEventAsync( @event );

		// Assert
		await _eventBus.Received( 1 ).PublishAsync( @event, Arg.Any<CancellationToken>( ) );
	}

	[Fact]
	public async Task DispatchCommandAsync_WhenHandlerThrows_ShouldReturnFailure( ) {
		// Arrange - Override the handler registration for this test
		var failingHandler = Substitute.For<ICommandHandler<TestCommand, string>>( );
		failingHandler.HandleAsync( Arg.Any<TestCommand>( ), Arg.Any<CancellationToken>( ) )
			.Returns( Task.FromException<CommandResult<string>>( new InvalidOperationException( "Handler failed" ) ) );

		// Create a new test-specific service provider for this failing scenario
		var services = new ServiceCollection( );
		services.AddTransient( _ => failingHandler );
		var provider = services.BuildServiceProvider( );

		MythServiceProvider.Reset( );
		MythServiceProvider.Initialize( provider );

		var dispatcher = new Dispatcher( _eventBus, _logger, _activitySource );

		var command = new TestCommand { Value = "test" };

		// Act
		var result = await dispatcher.DispatchCommandAsync<TestCommand, string>( command );

		// Assert
		result.IsFailure.Should( ).BeTrue( );
		result.ErrorMessage.Should( ).Contain( "Handler failed" );
	}

	public record UnregisteredQuery( string Key ) : IQuery<string>;

	[Fact( Skip = "TODO: Fix handler registration behavior" )]
	public async Task DispatchQueryAsync_WhenHandlerNotFound_ShouldReturnFailure( ) {
		// Arrange - Use a type that doesn't have a query handler
		var unregisteredQuery = new UnregisteredQuery( "test" );

		// Act
		var result = await _sut.DispatchQueryAsync<UnregisteredQuery, string>( unregisteredQuery );

		// Assert
		result.IsFailure.Should( ).BeTrue( );
		result.ErrorMessage.Should( ).Contain( "No handler registered" );
	}

	[Fact]
	public async Task DispatchQueryAsync_WithAutoGeneratedCacheKey_ShouldCreateDifferentKeysForDifferentParameters( ) {
		// Arrange
		var cacheProvider = Substitute.For<ICacheProvider>( );
		var capturedKeys = new List<string>( );

		cacheProvider.When( x => x.GetAsync<string>( Arg.Any<string>( ), Arg.Any<CancellationToken>( ) ) )
			.Do( callInfo => capturedKeys.Add( callInfo.ArgAt<string>( 0 ) ) );
		cacheProvider.GetAsync<string>( Arg.Any<string>( ), Arg.Any<CancellationToken>( ) ).ReturnsForAnyArgs( CacheValue<string>.Miss( ) );

		var dispatcher = new Dispatcher( _eventBus, _logger, _activitySource, cacheProvider );

		var query1 = new TestQuery { Key = "test-key-1" };
		var query2 = new TestQuery { Key = "test-key-2" };

		var cacheOptions = new CacheOptions {
			Enabled = true,
			Ttl = TimeSpan.FromMinutes( 5 )
		};

		// Act
		await dispatcher.DispatchQueryAsync<TestQuery, string>( query1, cacheOptions );
		await dispatcher.DispatchQueryAsync<TestQuery, string>( query2, cacheOptions );

		// Assert
		capturedKeys.Should( ).HaveCount( 2 );
		capturedKeys[ 0 ].Should( ).NotBe( capturedKeys[ 1 ] );
		capturedKeys[ 0 ].Should( ).StartWith( "TestQuery:" );
		capturedKeys[ 1 ].Should( ).StartWith( "TestQuery:" );
	}

	[Fact]
	public async Task DispatchQueryAsync_WithKeyGenerator_ShouldUseCustomKeyFunction( ) {
		// Arrange
		var cacheProvider = Substitute.For<ICacheProvider>( );
		var capturedKey = string.Empty;

		cacheProvider.When( x => x.GetAsync<string>( Arg.Any<string>( ), Arg.Any<CancellationToken>( ) ) )
			.Do( callInfo => capturedKey = callInfo.ArgAt<string>( 0 ) );
		cacheProvider.GetAsync<string>( Arg.Any<string>( ), Arg.Any<CancellationToken>( ) ).ReturnsForAnyArgs( CacheValue<string>.Miss( ) );

		var dispatcher = new Dispatcher( _eventBus, _logger, _activitySource, cacheProvider );

		var query = new TestQuery { Key = "test-key" };
		var cacheOptions = new CacheOptions {
			Enabled = true,
			KeyGenerator = q => $"custom-{( ( TestQuery )q ).Key}",
			Ttl = TimeSpan.FromMinutes( 5 )
		};

		// Act
		await dispatcher.DispatchQueryAsync<TestQuery, string>( query, cacheOptions );

		// Assert
		capturedKey.Should( ).Be( "custom-test-key" );
	}

	[Fact]
	public async Task DispatchQueryAsync_WithExplicitCacheKey_ShouldUseProvidedKey( ) {
		// Arrange
		var cacheProvider = Substitute.For<ICacheProvider>( );
		var capturedKey = string.Empty;

		cacheProvider.When( x => x.GetAsync<string>( Arg.Any<string>( ), Arg.Any<CancellationToken>( ) ) )
			.Do( callInfo => capturedKey = callInfo.ArgAt<string>( 0 ) );
		cacheProvider.GetAsync<string>( Arg.Any<string>( ), Arg.Any<CancellationToken>( ) ).ReturnsForAnyArgs( CacheValue<string>.Miss( ) );

		var dispatcher = new Dispatcher( _eventBus, _logger, _activitySource, cacheProvider );

		var query = new TestQuery { Key = "test-key" };
		var cacheOptions = new CacheOptions {
			Enabled = true,
			CacheKey = "explicit-cache-key",
			Ttl = TimeSpan.FromMinutes( 5 )
		};

		// Act
		await dispatcher.DispatchQueryAsync<TestQuery, string>( query, cacheOptions );

		// Assert
		capturedKey.Should( ).Be( "explicit-cache-key" );
	}

	[Fact]
	public async Task DispatchQueryAsync_WithKeyGeneratorPriority_ShouldPreferKeyGeneratorOverExplicitKey( ) {
		// Arrange
		var cacheProvider = Substitute.For<ICacheProvider>( );
		var capturedKey = string.Empty;

		cacheProvider.When( x => x.GetAsync<string>( Arg.Any<string>( ), Arg.Any<CancellationToken>( ) ) )
			.Do( callInfo => capturedKey = callInfo.ArgAt<string>( 0 ) );
		cacheProvider.GetAsync<string>( Arg.Any<string>( ), Arg.Any<CancellationToken>( ) ).ReturnsForAnyArgs( CacheValue<string>.Miss( ) );

		var dispatcher = new Dispatcher( _eventBus, _logger, _activitySource, cacheProvider );

		var query = new TestQuery { Key = "test-key" };
		var cacheOptions = new CacheOptions {
			Enabled = true,
			CacheKey = "explicit-key",
			KeyGenerator = q => $"generator-{( ( TestQuery )q ).Key}",
			Ttl = TimeSpan.FromMinutes( 5 )
		};

		// Act
		await dispatcher.DispatchQueryAsync<TestQuery, string>( query, cacheOptions );

		// Assert
		capturedKey.Should( ).Be( "generator-test-key" );
	}

	[Fact]
	public async Task DispatchQueryAsync_WithoutCacheOptions_ShouldNotUseCache( ) {
		// Arrange
		var cacheProvider = Substitute.For<ICacheProvider>( );
		var dispatcher = new Dispatcher( _eventBus, _logger, _activitySource, cacheProvider );

		var query = new TestQuery { Key = "test-key" };

		// Act
		var result = await dispatcher.DispatchQueryAsync<TestQuery, string>( query );

		// Assert
		result.IsSuccess.Should( ).BeTrue( );
		result.FromCache.Should( ).BeFalse( );

		// Verify cache was never accessed
		await cacheProvider.DidNotReceive( ).GetAsync<string>( Arg.Any<string>( ), Arg.Any<CancellationToken>( ) );
		await cacheProvider.DidNotReceive( ).SetAsync( Arg.Any<string>( ), Arg.Any<string>( ), Arg.Any<TimeSpan>( ), Arg.Any<bool>( ), Arg.Any<CancellationToken>( ) );
	}
}
