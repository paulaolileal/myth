using Myth.Flow.Test.Models;
using Xunit;

namespace Myth.Flow.Test;

public class PipelineStartTests : BaseTestFixture {

	[Fact]
	public void Start_WithInputOnly_ShouldCreatePipeline( ) {
		// Arrange
		var dto = new TestDto { Value = 42 };

		// Act
		var pipeline = Pipeline.Start( dto );

		// Assert
		Assert.NotNull( pipeline );
	}

	[Fact]
	public void Start_WithServiceProvider_ShouldCreatePipeline( ) {
		// Arrange
		var dto = new TestDto { Value = 42 };

		// Act
		var pipeline = Pipeline.Start( dto );

		// Assert
		Assert.NotNull( pipeline );
	}

	[Fact]
	public void Start_WithConfiguration_ShouldCreatePipelineWithConfig( ) {
		// Arrange
		var dto = new TestDto { Value = 42 };

		// Act
		var pipeline = Pipeline.Start( dto, config => {
			config.EnableTelemetry = false;
		} );

		// Assert
		Assert.NotNull( pipeline );
	}
}
