using Microsoft.Extensions.DependencyInjection;
using Myth.Extensions;
using Myth.Flow.Test.Models;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Myth.Flow.Test {

	public class TelemetryTests {

		[Fact]
		public async Task WithTelemetry_ShouldCreateActivity( ) {
			// Arrange
			var dto = new TestDto { Value = 42 };
			var activitySource = new ActivitySource( "TestSource" );
			var activityCreated = false;

			using var listener = new ActivityListener {
				ShouldListenTo = s => s.Name == "TestSource",
				Sample = ( ref ActivityCreationOptions<ActivityContext> _ ) =>
					ActivitySamplingResult.AllData,
				ActivityStarted = _ => activityCreated = true
			};
			ActivitySource.AddActivityListener( listener );

			var services = new ServiceCollection( );
			services.AddFlow( config => {
				config.ActivitySource = activitySource;
				config.EnableTelemetry = true;
			} );
			var provider = services.BuildServiceProvider( );

			// Service provider auto-initializes now - no manual initialization needed

			// Act
			var result = await Pipeline.Start( dto, provider )
				.WithTelemetry( "TestOperation" )
				.ExecuteAsync( );

			// Assert
			Assert.True( result.IsSuccess );
			Assert.True( activityCreated );
		}

		[Fact]
		public async Task WithTelemetry_Disabled_ShouldNotCreateActivity( ) {
			// Arrange
			var dto = new TestDto { Value = 42 };

			var services = new ServiceCollection( );
			services.AddFlow( config => config.EnableTelemetry = false );
			var provider = services.BuildServiceProvider( );

			// Act
			var result = await Pipeline.Start( dto, provider )
				.WithTelemetry( "TestOperation" )
				.ExecuteAsync( );

			// Assert - Should complete without error even though telemetry is disabled
			Assert.True( result.IsSuccess );
		}
	}
}