using FluentAssertions;
using Myth.Flow.Actions.Settings;

namespace Myth.Flow.Actions.Test {

	public class FlowActionsBuilderTests {

		[Fact]
		public void UseInMemory_ShouldSetBrokerType( ) {
			// Arrange
			var builder = new FlowActionsBuilder( );

			// Act
			builder.UseInMemory( );
			var config = builder.Build( );

			// Assert
			config.BrokerType.Should( ).Be( MessageBrokerType.InMemory );
		}

		[Fact]
		public void UseKafka_ShouldSetBrokerTypeAndConfiguration( ) {
			// Arrange
			var builder = new FlowActionsBuilder( );

			// Act
			builder.UseKafka( kafka => {
				kafka.BootstrapServers = "localhost:9092";
				kafka.GroupId = "test-group";
			} );
			var config = builder.Build( );

			// Assert
			config.BrokerType.Should( ).Be( MessageBrokerType.Kafka );
			config.BrokerConfiguration.Should( ).NotBeNull( );
		}

		[Fact]
		public void UseRabbitMQ_ShouldSetBrokerTypeAndConfiguration( ) {
			// Arrange
			var builder = new FlowActionsBuilder( );

			// Act
			builder.UseRabbitMQ( rabbit => {
				rabbit.HostName = "localhost";
				rabbit.UserName = "guest";
				rabbit.Password = "guest";
			} );
			var config = builder.Build( );

			// Assert
			config.BrokerType.Should( ).Be( MessageBrokerType.RabbitMQ );
			config.BrokerConfiguration.Should( ).NotBeNull( );
		}

		[Fact]
		public void EnableTelemetry_ShouldSetFlag( ) {
			// Arrange
			var builder = new FlowActionsBuilder( );

			// Act
			builder.EnableTelemetry( true );
			var config = builder.Build( );

			// Assert
			config.TelemetryEnabled.Should( ).BeTrue( );
		}

		[Fact]
		public void EnableCaching_ShouldSetFlag( ) {
			// Arrange
			var builder = new FlowActionsBuilder( );

			// Act
			builder.EnableCaching( cache => {
				cache.ProviderType = CacheProviderType.Memory;
				cache.DefaultTtl = TimeSpan.FromMinutes( 5 );
			} );
			var config = builder.Build( );

			// Assert
			config.CachingEnabled.Should( ).BeTrue( );
			config.CacheConfiguration.Should( ).NotBeNull( );
		}

		[Fact]
		public void EnableRetry_ShouldConfigureRetryPolicy( ) {
			// Arrange
			var builder = new FlowActionsBuilder( );

			// Act
			builder.EnableRetry( retry => {
				retry.MaxAttempts = 5;
				retry.BackoffMs = 2000;
				retry.ExponentialBackoff = true;
			} );
			var config = builder.Build( );

			// Assert
			config.RetryConfig.MaxAttempts.Should( ).Be( 5 );
			config.RetryConfig.BackoffMs.Should( ).Be( 2000 );
			config.RetryConfig.ExponentialBackoff.Should( ).BeTrue( );
		}

		[Fact]
		public void EnableDeadLetterQueue_ShouldSetFlag( ) {
			// Arrange
			var builder = new FlowActionsBuilder( );

			// Act
			builder.EnableDeadLetterQueue( );
			var config = builder.Build( );

			// Assert
			config.DeadLetterQueueEnabled.Should( ).BeTrue( );
		}

		[Fact]
		public void ScanAssemblies_ShouldAddAssemblies( ) {
			// Arrange
			var builder = new FlowActionsBuilder( );
			var assembly = typeof( FlowActionsBuilderTests ).Assembly;

			// Act
			builder.ScanAssemblies( assembly );
			var config = builder.Build( );

			// Assert
			config.AssembliesToScan.Should( ).Contain( assembly );
		}

		[Fact]
		public void Build_ShouldChainMethods( ) {
			// Arrange & Act
			var config = new FlowActionsBuilder( )
				.UseInMemory( )
				.EnableTelemetry( )
				.EnableCaching( )
				.EnableRetry( r => r.MaxAttempts = 3 )
				.EnableDeadLetterQueue( )
				.ScanAssemblies( GetType( ).Assembly )
				.Build( );

			// Assert
			config.BrokerType.Should( ).Be( MessageBrokerType.InMemory );
			config.TelemetryEnabled.Should( ).BeTrue( );
			config.CachingEnabled.Should( ).BeTrue( );
			config.RetryConfig.MaxAttempts.Should( ).Be( 3 );
			config.DeadLetterQueueEnabled.Should( ).BeTrue( );
			config.AssembliesToScan.Should( ).NotBeEmpty( );
		}
	}
}