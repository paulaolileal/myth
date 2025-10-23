using Microsoft.Extensions.DependencyInjection;
using Myth.Flow.Test.Models;
using System.Threading.Tasks;
using Xunit;

namespace Myth.Flow.Test {

	public class TapTests {

		[Fact]
		public async Task Tap_ShouldExecuteSideEffect( ) {
			// Arrange
			var dto = new TestDto { Value = 42 };
			var sideEffectExecuted = false;

			// Act
			var result = await Pipeline.Start( dto, new ServiceCollection( ).BuildServiceProvider( ) )
				.Tap( d => sideEffectExecuted = true )
				.ExecuteAsync( );

			// Assert
			Assert.True( result.IsSuccess );
			Assert.True( sideEffectExecuted );
			Assert.Equal( 42, result.Value!.Value );
		}

		[Fact]
		public async Task TapAsync_ShouldExecuteAsyncSideEffect( ) {
			// Arrange
			var dto = new TestDto { Value = 42 };
			var sideEffectValue = 0;

			// Act
			var result = await Pipeline.Start( dto, new ServiceCollection( ).BuildServiceProvider( ) )
				.TapAsync( async d => {
					await Task.Delay( 10 );
					sideEffectValue = d.Value;
				} )
				.ExecuteAsync( );

			// Assert
			Assert.True( result.IsSuccess );
			Assert.Equal( 42, sideEffectValue );
			Assert.Equal( 42, result.Value!.Value );
		}

		[Fact]
		public async Task Tap_ShouldNotModifyContext( ) {
			// Arrange
			var dto = new TestDto { Value = 42 };

			// Act
			var result = await Pipeline.Start( dto, new ServiceCollection( ).BuildServiceProvider( ) )
				.Tap( d => d.Value = 100 ) // Try to modify
				.ExecuteAsync( );

			// Assert
			Assert.True( result.IsSuccess );
			Assert.Equal( 100, result.Value!.Value ); // Modified by reference
		}
	}
}