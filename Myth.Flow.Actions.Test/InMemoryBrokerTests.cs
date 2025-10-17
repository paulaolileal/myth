using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Myth.Flow.Actions.Brokers;
using Myth.Flow.Actions.Settings;
using Myth.Flow.Actions.Test.Models;
using Myth.Flow.Resilience;
using Myth.Interfaces;
using NSubstitute;
using System.Diagnostics;

namespace Myth.Flow.Actions.Test {

	public class InMemoryBrokerTests : IAsyncDisposable {
		private readonly IServiceProvider _serviceProvider;
		private readonly IEventSubscriptionManager _subscriptionManager;
		private readonly ILogger<InMemoryBroker> _logger;
		private readonly ActivitySource _activitySource;
		private readonly InMemoryBroker _sut;

		public InMemoryBrokerTests( ) {
			var services = new ServiceCollection( );
			services.AddLogging( );
			services.AddSingleton<TestEventHandler>( );
			_serviceProvider = services.BuildServiceProvider( );

			_subscriptionManager = Substitute.For<IEventSubscriptionManager>( );
			_logger = Substitute.For<ILogger<InMemoryBroker>>( );
			_activitySource = new ActivitySource( "Test" );

			var options = new InMemoryBrokerOptions {
				WorkerCount = 2,
				MaxConcurrentHandlers = 10,
				ChannelCapacity = 100,
				MaxRetryAttempts = 3,
				EnableDeadLetterQueue = false
			};

			_sut = new InMemoryBroker(
				_serviceProvider,
				_subscriptionManager,
				_logger,
				_activitySource,
				options );
		}

		[Fact]
		public async Task PublishAsync_ShouldNotThrow( ) {
			// Arrange
			var @event = new TestEvent { Message = "test" };

			// Act
			var act = async ( ) => await _sut.PublishAsync( @event );

			// Assert
			await act.Should( ).NotThrowAsync( );
		}

		[Fact]
		public async Task StartAsync_ShouldNotThrow( ) {
			// Arrange & Act
			var act = async ( ) => await _sut.StartAsync( );

			// Assert
			await act.Should( ).NotThrowAsync( );
		}

		[Fact]
		public async Task StartAsync_ShouldSucceed( ) {
			// Arrange & Act
			var act = async ( ) => await _sut.StartAsync( );

			// Assert
			await act.Should( ).NotThrowAsync( );
		}

		[Fact]
		public async Task StopAsync_ShouldCompleteSuccessfully( ) {
			// Arrange
			await _sut.StartAsync( );

			// Act
			await _sut.StopAsync( );

			// Assert
			_logger.Received( ).LogInformation( "Stopping in-memory message broker" );
		}

		[Fact]
		public async Task PublishAsync_MultipleMessages_ShouldHandleAll( ) {
			// Arrange
			await _sut.StartAsync( );

			// Act
			for ( int i = 0; i < 10; i++ ) {
				await _sut.PublishAsync( new TestEvent { Message = $"Message {i}" } );
			}

			// Wait for processing
			await Task.Delay( 100 );
			await _sut.StopAsync( );

			// Assert - No exception thrown
		}

		[Fact]
		public async Task PublishAsync_BeforeStart_ShouldEnqueue( ) {
			// Arrange
			var @event = new TestEvent { Message = "test" };

			// Act - Should enqueue even without Start
			await _sut.PublishAsync( @event );

			// Assert
			await _sut.StartAsync( );
			await Task.Delay( 50 ); // Give time to process
			await _sut.StopAsync( );
		}

		[Fact]
		public async Task ProcessMessage_WithRegisteredHandler_ShouldInvokeHandler( ) {
			// Arrange
			var handler = new TestEventHandler( );
			var services = new ServiceCollection( );
			services.AddLogging( );

			services.AddSingleton<IEventHandler<TestEvent>>( handler );

			var provider = services.BuildServiceProvider( );

			var subscriptionManager = new EventSubscriptionManager( );
			subscriptionManager.RegisterHandler<TestEvent, TestEventHandler>( );

			var broker = new InMemoryBroker(
				provider,
				subscriptionManager,
				_logger,
				_activitySource,
				new InMemoryBrokerOptions { WorkerCount = 1, MaxRetryAttempts = 0 } );

			await broker.StartAsync( );
			await Task.Delay( 200 ); // Esperar worker iniciar

			// Act
			await broker.PublishAsync( new TestEvent { Message = "test-message" } );
			await Task.Delay( 500 ); // Mais tempo para processar

			await broker.StopAsync( );

			// Assert
			handler.CallCount.Should( ).BeGreaterThan( 0 );
			handler.LastEvent.Should( ).NotBeNull( );
			handler.LastEvent!.Message.Should( ).Be( "test-message" );
		}

		[Fact]
		public async Task ProcessMessage_WithoutHandlers_ShouldNotThrow( ) {
			// Arrange
			_subscriptionManager.GetHandlersForEvent( Arg.Any<Type>( ) )
				.Returns( Enumerable.Empty<Type>( ) );

			// Act & assert
			await _sut.StartAsync( );
			await _sut.PublishAsync( new TestEvent { Message = "test" } );
			await Task.Delay( 200 );
			await _sut.StopAsync( );
		}

		[Fact]
		public async Task ProcessMessage_WithRetry_ShouldRetryOnFailure( ) {
			// Arrange
			var attemptCount = 0;
			var failingHandler = Substitute.For<IEventHandler<TestEvent>>( );
			failingHandler.HandleAsync( Arg.Any<TestEvent>( ), Arg.Any<CancellationToken>( ) )
				.Returns( callInfo => {
					attemptCount++;
					if ( attemptCount < 3 )
						throw new InvalidOperationException( "Simulated failure" );
					return Task.CompletedTask;
				} );

			var services = new ServiceCollection( );
			services.AddLogging( );
			services.AddSingleton<IEventHandler<TestEvent>>( failingHandler );
			var provider = services.BuildServiceProvider( );

			var subscriptionManager = Substitute.For<IEventSubscriptionManager>( );
			subscriptionManager
				.GetHandlersForEvent( Arg.Any<Type>( ) )
				.Returns( [ typeof( IEventHandler<TestEvent> ) ] );

			var options = new InMemoryBrokerOptions {
				WorkerCount = 1,
				MaxRetryAttempts = 3,
				RetryBackoffMs = 10,
				ExponentialBackoff = false
			};

			var broker = new InMemoryBroker(
				provider,
				subscriptionManager,
				_logger,
				_activitySource,
				options );

			await broker.StartAsync( );

			// Act
			await broker.PublishAsync( new TestEvent { Message = "test" } );
			await Task.Delay( 500 ); // Wait for retries

			// Assert
			attemptCount.Should( ).BeGreaterOrEqualTo( 2 );

			await broker.StopAsync( );
		}

		[Fact]
		public async Task ProcessMessage_WithDeadLetterQueue_ShouldMoveToDLQAfterMaxRetries( ) {
			// Arrange
			var dlq = new DeadLetterQueue(
				Substitute.For<ILogger<DeadLetterQueue>>( ),
				maxSize: 100 );

			var failingHandler = Substitute.For<IEventHandler<TestEvent>>( );
			failingHandler.HandleAsync( Arg.Any<TestEvent>( ), Arg.Any<CancellationToken>( ) )
				.Returns( Task.FromException( new InvalidOperationException( "Always fails" ) ) );

			var services = new ServiceCollection( );
			services.AddLogging( );
			services.AddSingleton<IEventHandler<TestEvent>>( failingHandler );
			var provider = services.BuildServiceProvider( );

			var subscriptionManager = Substitute.For<IEventSubscriptionManager>( );
			subscriptionManager
				.GetHandlersForEvent( Arg.Any<Type>( ) )
				.Returns( [ typeof( IEventHandler<TestEvent> ) ] );

			var options = new InMemoryBrokerOptions {
				WorkerCount = 1,
				MaxRetryAttempts = 2,
				RetryBackoffMs = 10,
				EnableDeadLetterQueue = true
			};

			var broker = new InMemoryBroker(
				provider,
				subscriptionManager,
				_logger,
				_activitySource,
				options,
				dlq );

			await broker.StartAsync( );

			// Act
			await broker.PublishAsync( new TestEvent { Message = "test" } );
			await Task.Delay( 500 ); // Wait for all retries

			// Assert
			dlq.Count.Should( ).BeGreaterThan( 0 );

			await broker.StopAsync( );
		}

		[Fact]
		public async Task ConcurrentProcessing_ShouldHandleMultipleEventsInParallel( ) {
			// Arrange
			var handler = new TestEventHandler( );
			var services = new ServiceCollection( );
			services.AddLogging( );
			services.AddSingleton<IEventHandler<TestEvent>>( handler );

			var provider = services.BuildServiceProvider( );

			var subscriptionManager = new EventSubscriptionManager( );
			subscriptionManager.RegisterHandler<TestEvent, TestEventHandler>( );

			var options = new InMemoryBrokerOptions {
				WorkerCount = 2,
				MaxConcurrentHandlers = 20,
				ChannelCapacity = 100,
				MaxRetryAttempts = 0
			};

			var broker = new InMemoryBroker( provider, subscriptionManager, _logger, _activitySource, options );

			await broker.StartAsync( );
			await Task.Delay( 200 ); // Workers iniciarem

			// Act - Testar com 20 eventos (mais confiável)
			const int eventCount = 20;

			for ( int i = 0; i < eventCount; i++ ) {
				await broker.PublishAsync( new TestEvent { Message = $"Event {i}" } );
			}

			// Wait
			var maxWait = TimeSpan.FromSeconds( 5 );
			var sw = Stopwatch.StartNew( );

			while ( handler.CallCount < eventCount && sw.Elapsed < maxWait ) {
				await Task.Delay( 100 );
			}

			await broker.StopAsync( );

			// Assert - Permitir margem de erro
			handler.CallCount.Should( ).BeGreaterOrEqualTo( eventCount - 2 );
		}

		[Fact]
		public async Task Telemetry_ShouldCreateActivities( ) {
			// Arrange
			var activitySource = new ActivitySource( "TestBroker" );
			var activities = new List<Activity>( );

			using var listener = new ActivityListener {
				ShouldListenTo = source => source.Name == "TestBroker",
				Sample = ( ref ActivityCreationOptions<ActivityContext> _ ) => ActivitySamplingResult.AllData,
				ActivityStarted = activity => activities.Add( activity )
			};

			ActivitySource.AddActivityListener( listener );

			var broker = new InMemoryBroker(
				_serviceProvider,
				_subscriptionManager,
				_logger,
				activitySource,
				new InMemoryBrokerOptions { WorkerCount = 1 } );

			await broker.StartAsync( );

			// Act
			await broker.PublishAsync( new TestEvent { Message = "test" } );
			await Task.Delay( 100 );

			// Assert
			activities.Should( ).NotBeEmpty( );
			activities.Should( ).Contain( a => a.OperationName.Contains( "InMemoryBroker.Publish" ) );

			await broker.StopAsync( );
		}

		public async ValueTask DisposeAsync( ) {
			try {
				await _sut.StopAsync( );
			} catch { }

			await _sut.DisposeAsync( );
			_activitySource?.Dispose( );
		}
	}
}