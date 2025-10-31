using Microsoft.Extensions.DependencyInjection;
using Myth.Extensions;
using Myth.Flow;
using Myth.ServiceProvider;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Myth.Flow.Test {

	public class ExceptionFilterIntegrationTests : IDisposable {
		private readonly IServiceProvider _serviceProvider;

		public ExceptionFilterIntegrationTests( ) {
			var services = new ServiceCollection( );
			services.AddFlow( config => config
				.UseLogging( )
				.UseTelemetry( )
				.UseExceptionFilter<ArgumentException>( )
				.UseExceptionFilter<InvalidOperationException>( ) );

			_serviceProvider = services.BuildServiceProvider( );
			MythServiceProvider.Initialize( _serviceProvider );
		}

		[Fact]
		public async Task Pipeline_WithFilteredException_ShouldPropagateException( ) {
			// Arrange
			var input = "test";

			// Act & Assert
			var action = async ( ) => await Pipeline.Start( input )
				.StepAsync( async ctx => {
					await Task.Delay( 10 );
					throw new ArgumentException( "This should be propagated" );
#pragma warning disable CS0162 // Unreachable code detected
					return ctx;
#pragma warning restore CS0162 // Unreachable code detected
				} )
				.ExecuteAsync( );

			var ex = await Assert.ThrowsAsync<ArgumentException>( action );
			Assert.Contains( "This should be propagated", ex.Message );
		}

		[Fact]
		public async Task Pipeline_WithNonFilteredException_ShouldReturnFailureResult( ) {
			// Arrange
			var input = "test";

			// Act
			var result = await Pipeline.Start( input )
				.StepAsync( async ctx => {
					await Task.Delay( 10 );
					throw new InvalidDataException( "This should not be propagated" );
#pragma warning disable CS0162 // Unreachable code detected
					return ctx;
#pragma warning restore CS0162 // Unreachable code detected
				} )
				.ExecuteAsync( );

			// Assert
			Assert.True( result.IsFailure );
			Assert.Contains( "This should not be propagated", result.ErrorMessage ?? "" );
		}

		[Fact]
		public async Task Pipeline_WithInheritedException_ShouldPropagateException( ) {
			// Arrange
			var input = "test";

			// Act & Assert
			var action = async ( ) => await Pipeline.Start( input )
				.StepAsync( async ctx => {
					await Task.Delay( 10 );
					throw new ArgumentNullException( "param", "This inherits from ArgumentException" );
#pragma warning disable CS0162 // Unreachable code detected
					return ctx;
#pragma warning restore CS0162 // Unreachable code detected
				} )
				.ExecuteAsync( );

			var ex = await Assert.ThrowsAsync<ArgumentNullException>( action );
			Assert.Contains( "This inherits from ArgumentException", ex.Message );
		}

		[Fact]
		public async Task Pipeline_WithoutExceptionFilter_ShouldHandleAllExceptions( ) {
			// Arrange
			var services = new ServiceCollection( );
			services.AddFlow( ); // No exception filter configured

			var localServiceProvider = services.BuildServiceProvider( );
			MythServiceProvider.Initialize( localServiceProvider );

			var input = "test";

			// Act
			var result = await Pipeline.Start( input )
				.StepAsync( async ctx => {
					await Task.Delay( 10 );
					throw new ArgumentException( "This should be handled" );
#pragma warning disable CS0162 // Unreachable code detected
					return ctx;
#pragma warning restore CS0162 // Unreachable code detected
				} )
				.ExecuteAsync( );

			// Assert
			Assert.True( result.IsFailure );
			Assert.Contains( "This should be handled", result.ErrorMessage ?? "" );
		}

		[Fact]
		public async Task Pipeline_WithMultipleSteps_ShouldPropagateFromAnyStep( ) {
			// Arrange
			var input = "test";

			// Act & Assert
			var action = async ( ) => await Pipeline.Start( input )
				.StepAsync( async ctx => {
					await Task.Delay( 10 );
					return ctx + "_step1";
				} )
				.StepAsync( async ctx => {
					await Task.Delay( 10 );
					throw new InvalidOperationException( "Second step failure" );
#pragma warning disable CS0162 // Unreachable code detected
					return ctx;
#pragma warning restore CS0162 // Unreachable code detected
				} )
				.ExecuteAsync( );

			var ex = await Assert.ThrowsAsync<InvalidOperationException>( action );
			Assert.Contains( "Second step failure", ex.Message );
		}

		public void Dispose( ) {
			if ( _serviceProvider is IDisposable disposable ) {
				disposable.Dispose( );
			}
		}
	}
}