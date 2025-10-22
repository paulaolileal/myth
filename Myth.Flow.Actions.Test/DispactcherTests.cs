using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Myth.Flow.Actions.Test.Models;
using Myth.Interfaces;
using Myth.Models;
using NSubstitute;
using System.Diagnostics;

namespace Myth.Flow.Actions.Test {

	public class DispatcherTests {
		private readonly IServiceProvider _serviceProvider;
		private readonly IEventBus _eventBus;
		private readonly ILogger<Dispatcher> _logger;
		private readonly ActivitySource _activitySource;
		private readonly Dispatcher _sut;

		public DispatcherTests( ) {
			var services = new ServiceCollection( );

			services.AddTransient<ICommandHandler<TestCommand, string>, TestCommandHandler>( );
			services.AddTransient<ICommandHandler<TestCommandNoResponse>, TestCommandNoResponseHandler>( );
			services.AddTransient<IQueryHandler<TestQuery, string>, TestQueryHandler>( );

			_serviceProvider = services.BuildServiceProvider( );

			_eventBus = Substitute.For<IEventBus>( );
			_logger = Substitute.For<ILogger<Dispatcher>>( );
			_activitySource = new ActivitySource( "Test" );
			_sut = new Dispatcher( _serviceProvider, _eventBus, _logger, _activitySource );
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

		[Fact]
		public async Task DispatchCommandAsync_WhenHandlerNotRegistered_ShouldReturnFailure( ) {
			// Arrange
			var command = new TestCommand { Value = "test" };
			var emptyServices = new ServiceCollection( ).BuildServiceProvider( );
			var dispatcher = new Dispatcher( emptyServices, _eventBus, _logger, _activitySource );

			// Act
			var result = await dispatcher.DispatchCommandAsync<TestCommand, string>( command );

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
			var services = new ServiceCollection( );
			services.AddTransient<IQueryHandler<TestQuery, string>, TestQueryHandler>( );
			services.AddMemoryCache( );

			var cacheProvider = Substitute.For<ICacheProvider>( );
			cacheProvider.GetAsync<string>( Arg.Any<string>( ), Arg.Any<CancellationToken>( ) ).ReturnsForAnyArgs( CacheValue<string>.Hit( "cached-value" ) );

			var provider = services.BuildServiceProvider( );
			var dispatcher = new Dispatcher( provider, _eventBus, _logger, _activitySource, cacheProvider );

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
			// Arrange
			var services = new ServiceCollection( );
			var failingHandler = Substitute.For<ICommandHandler<TestCommand, string>>( );
			failingHandler.HandleAsync( Arg.Any<TestCommand>( ), Arg.Any<CancellationToken>( ) )
				.Returns( Task.FromException<CommandResult<string>>( new InvalidOperationException( "Handler failed" ) ) );

			services.AddTransient( _ => failingHandler );
			var provider = services.BuildServiceProvider( );
			var dispatcher = new Dispatcher( provider, _eventBus, _logger, _activitySource );

			var command = new TestCommand { Value = "test" };

			// Act
			var result = await dispatcher.DispatchCommandAsync<TestCommand, string>( command );

			// Assert
			result.IsFailure.Should( ).BeTrue( );
			result.ErrorMessage.Should( ).Contain( "Handler failed" );
		}

		[Fact]
		public async Task DispatchQueryAsync_WhenHandlerNotFound_ShouldReturnFailure( ) {
			// Arrange
			var emptyServices = new ServiceCollection( ).BuildServiceProvider( );
			var dispatcher = new Dispatcher( emptyServices, _eventBus, _logger, _activitySource );
			var query = new TestQuery { Key = "test" };

			// Act
			var result = await dispatcher.DispatchQueryAsync<TestQuery, string>( query );

			// Assert
			result.IsFailure.Should( ).BeTrue( );
			result.ErrorMessage.Should( ).Contain( "No handler registered" );
		}

		[Fact]
		public async Task DispatchQueryAsync_WithAutoGeneratedCacheKey_ShouldCreateDifferentKeysForDifferentParameters( ) {
			// Arrange
			var services = new ServiceCollection( );
			services.AddTransient<IQueryHandler<TestQuery, string>, TestQueryHandler>( );

			var cacheProvider = Substitute.For<ICacheProvider>( );
			var capturedKeys = new List<string>( );

			cacheProvider.When( x => x.GetAsync<string>( Arg.Any<string>( ), Arg.Any<CancellationToken>( ) ) )
				.Do( callInfo => capturedKeys.Add( callInfo.ArgAt<string>( 0 ) ) );
			cacheProvider.GetAsync<string>( Arg.Any<string>( ), Arg.Any<CancellationToken>( ) ).ReturnsForAnyArgs( CacheValue<string>.Miss( ) );

			var provider = services.BuildServiceProvider( );
			var dispatcher = new Dispatcher( provider, _eventBus, _logger, _activitySource, cacheProvider );

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
			var services = new ServiceCollection( );
			services.AddTransient<IQueryHandler<TestQuery, string>, TestQueryHandler>( );

			var cacheProvider = Substitute.For<ICacheProvider>( );
			var capturedKey = string.Empty;

			cacheProvider.When( x => x.GetAsync<string>( Arg.Any<string>( ), Arg.Any<CancellationToken>( ) ) )
				.Do( callInfo => capturedKey = callInfo.ArgAt<string>( 0 ) );
			cacheProvider.GetAsync<string>( Arg.Any<string>( ), Arg.Any<CancellationToken>( ) ).ReturnsForAnyArgs( CacheValue<string>.Miss( ) );

			var provider = services.BuildServiceProvider( );
			var dispatcher = new Dispatcher( provider, _eventBus, _logger, _activitySource, cacheProvider );

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
			var services = new ServiceCollection( );
			services.AddTransient<IQueryHandler<TestQuery, string>, TestQueryHandler>( );

			var cacheProvider = Substitute.For<ICacheProvider>( );
			var capturedKey = string.Empty;

			cacheProvider.When( x => x.GetAsync<string>( Arg.Any<string>( ), Arg.Any<CancellationToken>( ) ) )
				.Do( callInfo => capturedKey = callInfo.ArgAt<string>( 0 ) );
			cacheProvider.GetAsync<string>( Arg.Any<string>( ), Arg.Any<CancellationToken>( ) ).ReturnsForAnyArgs( CacheValue<string>.Miss( ) );

			var provider = services.BuildServiceProvider( );
			var dispatcher = new Dispatcher( provider, _eventBus, _logger, _activitySource, cacheProvider );

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
			var services = new ServiceCollection( );
			services.AddTransient<IQueryHandler<TestQuery, string>, TestQueryHandler>( );

			var cacheProvider = Substitute.For<ICacheProvider>( );
			var capturedKey = string.Empty;

			cacheProvider.When( x => x.GetAsync<string>( Arg.Any<string>( ), Arg.Any<CancellationToken>( ) ) )
				.Do( callInfo => capturedKey = callInfo.ArgAt<string>( 0 ) );
			cacheProvider.GetAsync<string>( Arg.Any<string>( ), Arg.Any<CancellationToken>( ) ).ReturnsForAnyArgs( CacheValue<string>.Miss( ) );

			var provider = services.BuildServiceProvider( );
			var dispatcher = new Dispatcher( provider, _eventBus, _logger, _activitySource, cacheProvider );

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
			var services = new ServiceCollection( );
			services.AddTransient<IQueryHandler<TestQuery, string>, TestQueryHandler>( );

			var cacheProvider = Substitute.For<ICacheProvider>( );
			var provider = services.BuildServiceProvider( );
			var dispatcher = new Dispatcher( provider, _eventBus, _logger, _activitySource, cacheProvider );

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
}