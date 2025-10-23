using Microsoft.Extensions.DependencyInjection;
using Myth.Flow.Test.Models;
using Xunit;

namespace Myth.Flow.Test {

	public class PipelineStartTests {

		[Fact]
		public void Start_WithInputOnly_ShouldCreatePipeline( ) {
			// Arrange
			var dto = new TestDto { Value = 42 };

			// Act
			var pipeline = Pipeline.Start( dto, new ServiceCollection( ).BuildServiceProvider( ) );

			// Assert
			Assert.NotNull( pipeline );
		}

		[Fact]
		public void Start_WithServiceProvider_ShouldCreatePipeline( ) {
			// Arrange
			var dto = new TestDto { Value = 42 };
			var services = new ServiceCollection( );
			var provider = services.BuildServiceProvider( );

			// Act
			var pipeline = Pipeline.Start( dto, provider );

			// Assert
			Assert.NotNull( pipeline );
		}

		[Fact]
		public void Start_WithConfiguration_ShouldCreatePipelineWithConfig( ) {
			// Arrange
			var dto = new TestDto { Value = 42 };
			var services = new ServiceCollection( );
			var provider = services.BuildServiceProvider( );

			// Act
			var pipeline = Pipeline.Start( dto, provider, config => {
				config.EnableTelemetry = false;
			} );

			// Assert
			Assert.NotNull( pipeline );
		}
	}
}