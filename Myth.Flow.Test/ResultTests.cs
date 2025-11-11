using System;
using Myth.Flow.Test.Models;
using Myth.Models;
using Xunit;

namespace Myth.Flow.Test;

public class ResultTests {

	[Fact]
	public void Success_ShouldCreateSuccessResult( ) {
		// Arrange
		var value = new TestDto { Value = 42 };

		// Act
		var result = Result<TestDto>.Success( value );

		// Assert
		Assert.True( result.IsSuccess );
		Assert.False( result.IsFailure );
		Assert.Equal( value, result.Value );
		Assert.Null( result.ErrorMessage );
		Assert.Null( result.Exception );
	}

	[Fact]
	public void Failure_WithMessageOnly_ShouldCreateFailureResult( ) {
		// Arrange
		var errorMessage = "Test error";

		// Act
		var result = Result<TestDto>.Failure( errorMessage );

		// Assert
		Assert.False( result.IsSuccess );
		Assert.True( result.IsFailure );
		Assert.Null( result.Value );
		Assert.Equal( errorMessage, result.ErrorMessage );
		Assert.Null( result.Exception );
	}

	[Fact]
	public void Failure_WithException_ShouldCreateFailureResultWithException( ) {
		// Arrange
		var errorMessage = "Test error";
		var exception = new InvalidOperationException( "Inner error" );

		// Act
		var result = Result<TestDto>.Failure( errorMessage, exception );

		// Assert
		Assert.False( result.IsSuccess );
		Assert.True( result.IsFailure );
		Assert.Null( result.Value );
		Assert.Equal( errorMessage, result.ErrorMessage );
		Assert.Equal( exception, result.Exception );
	}
}
