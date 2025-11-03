using Myth.Builders;
using System;
using Xunit;

namespace Myth.Flow.Test {

	public class FlowConfigurationBuilderTests {

		[Fact]
		public void Builder_ShouldCreateDefaultConfiguration( ) {
			// Arrange
			var builder = new FlowConfigurationBuilder( );

			// Act
			var config = builder.Build( );

			// Assert
			Assert.True( config.EnableTelemetry );
			Assert.True( config.EnableLogging );
			Assert.Equal( 0, config.DefaultRetryAttempts );
			Assert.Equal( 100, config.DefaultBackoffMs );
			Assert.Null( config.ActivitySource );
			Assert.Empty( config.ExceptionTypesToPropagate );
		}

		[Fact]
		public void Builder_ShouldAllowFluentConfiguration( ) {
			// Arrange
			var builder = new FlowConfigurationBuilder( );

			// Act
			var config = builder
				.DisableTelemetry( )
				.DisableLogging( )
				.UseRetry( 3, 500 )
				.UseActivitySource( "TestApp", "1.0" )
				.Build( );

			// Assert
			Assert.False( config.EnableTelemetry );
			Assert.False( config.EnableLogging );
			Assert.Equal( 3, config.DefaultRetryAttempts );
			Assert.Equal( 500, config.DefaultBackoffMs );
			Assert.NotNull( config.ActivitySource );
			Assert.Equal( "TestApp", config.ActivitySource!.Name );
		}

		[Fact]
		public void UseExceptionFilter_WithTypes_ShouldAddExceptionTypes( ) {
			// Arrange
			var builder = new FlowConfigurationBuilder( );

			// Act
			var config = builder
				.UseExceptionFilter( typeof( ArgumentException ), typeof( InvalidOperationException ) )
				.Build( );

			// Assert
			Assert.Equal( 2, config.ExceptionTypesToPropagate.Count );
			Assert.Contains( typeof( ArgumentException ), config.ExceptionTypesToPropagate );
			Assert.Contains( typeof( InvalidOperationException ), config.ExceptionTypesToPropagate );
		}

		[Fact]
		public void UseExceptionFilter_WithGeneric_ShouldAddExceptionType( ) {
			// Arrange
			var builder = new FlowConfigurationBuilder( );

			// Act
			var config = builder
				.UseExceptionFilter<ArgumentNullException>( )
				.UseExceptionFilter<ArgumentException>( )
				.Build( );

			// Assert
			Assert.Equal( 2, config.ExceptionTypesToPropagate.Count );
			Assert.Contains( typeof( ArgumentNullException ), config.ExceptionTypesToPropagate );
			Assert.Contains( typeof( ArgumentException ), config.ExceptionTypesToPropagate );
		}

		[Fact]
		public void UseExceptionFilter_WithNullTypes_ShouldThrowArgumentNullException( ) {
			// Arrange
			var builder = new FlowConfigurationBuilder( );

			// Act & Assert
			Assert.Throws<ArgumentNullException>( ( ) => builder.UseExceptionFilter( null! ) );
		}

		[Fact]
		public void UseExceptionFilter_WithNonExceptionType_ShouldIgnoreInvalidTypes( ) {
			// Arrange
			var builder = new FlowConfigurationBuilder( );

			// Act
			var config = builder
				.UseExceptionFilter( typeof( string ), typeof( ArgumentException ) )
				.Build( );

			// Assert
			Assert.Equal( 1, config.ExceptionTypesToPropagate.Count );
			Assert.Contains( typeof( ArgumentException ), config.ExceptionTypesToPropagate );
			Assert.DoesNotContain( typeof( string ), config.ExceptionTypesToPropagate );
		}

		[Fact]
		public void UseExceptionFilter_WithDuplicateTypes_ShouldNotAddDuplicates( ) {
			// Arrange
			var builder = new FlowConfigurationBuilder( );

			// Act
			var config = builder
				.UseExceptionFilter( typeof( ArgumentException ) )
				.UseExceptionFilter( typeof( ArgumentException ) )
				.Build( );

			// Assert
			Assert.Equal( 1, config.ExceptionTypesToPropagate.Count );
			Assert.Contains( typeof( ArgumentException ), config.ExceptionTypesToPropagate );
		}
	}
}