using Myth.Exceptions;
using System;
using Xunit;

namespace Myth.Flow.Test {

	public class PipelineExceptionTests {

		[Fact]
		public void PipelineException_WithMessage_ShouldCreateException( ) {
			// Arrange
			var message = "Test exception";

			// Act
			var exception = new PipelineException( message );

			// Assert
			Assert.Equal( message, exception.Message );
			Assert.Null( exception.InnerException );
		}

		[Fact]
		public void PipelineException_WithInnerException_ShouldCreateException( ) {
			// Arrange
			var message = "Test exception";
			var innerException = new InvalidOperationException( "Inner" );

			// Act
			var exception = new PipelineException( message, innerException );

			// Assert
			Assert.Equal( message, exception.Message );
			Assert.Equal( innerException, exception.InnerException );
		}
	}
}