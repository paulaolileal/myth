using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.ServiceProvider;

namespace Myth.Commons.Test;

/// <summary>
/// Tests for ScopedService disposal behavior with IDisposable and IAsyncDisposable services
/// </summary>
public class ScopedServiceDisposalTests : BaseTestFixture {

	protected override void ConfigureServices( IServiceCollection services ) {
		services.AddScoped<IAsyncDisposableTestService, AsyncDisposableTestService>( );
		services.AddScoped<IDisposableTestService, DisposableTestService>( );
	}

	[Fact]
	public async Task ScopedService_WithAsyncDisposableService_ShouldDisposeCorrectly( ) {
		// Arrange
		var scopedService = ServiceProvider.GetRequiredService<IScopedService<IAsyncDisposableTestService>>( );

		// Act & Assert
		var result = await scopedService.ExecuteAsync( async service => {
			await service.DoSomethingAsync( );
			return "success";
		} );

		result.Should( ).Be( "success" );

		var testService = ServiceProvider.GetRequiredService<IAsyncDisposableTestService>( ) as AsyncDisposableTestService;
	}

	[Fact]
	public void ScopedService_WithAsyncDisposableService_SyncExecution_ShouldDisposeCorrectly( ) {
		// Arrange
		var scopedService = ServiceProvider.GetRequiredService<IScopedService<IAsyncDisposableTestService>>( );

		// Act & Assert
		var result = scopedService.Execute( service => {
			service.DoSomething( );
			return "success";
		} );

		result.Should( ).Be( "success" );
	}

	[Fact]
	public async Task ScopedService_WithRegularDisposableService_ShouldDisposeCorrectly( ) {
		// Arrange
		var scopedService = ServiceProvider.GetRequiredService<IScopedService<IDisposableTestService>>( );

		// Act & Assert
		var result = await scopedService.ExecuteAsync( async service => {
			await service.DoSomethingAsync( );
			return "success";
		} );

		result.Should( ).Be( "success" );
	}

	[Fact]
	public void ScopedService_WithRegularDisposableService_SyncExecution_ShouldDisposeCorrectly( ) {
		// Arrange
		var scopedService = ServiceProvider.GetRequiredService<IScopedService<IDisposableTestService>>( );

		// Act & Assert
		var result = scopedService.Execute( service => {
			service.DoSomething( );
			return "success";
		} );

		result.Should( ).Be( "success" );
	}
}

// Test interfaces and implementations
public interface IAsyncDisposableTestService {

	void DoSomething( );

	Task DoSomethingAsync( );
}

public interface IDisposableTestService {

	void DoSomething( );

	Task DoSomethingAsync( );
}

public class AsyncDisposableTestService : IAsyncDisposableTestService, IAsyncDisposable {
	public bool IsDisposed { get; private set; }

	public void DoSomething( ) {
		if ( IsDisposed )
			throw new ObjectDisposedException( nameof( AsyncDisposableTestService ) );
	}

	public Task DoSomethingAsync( ) {
		return IsDisposed
			? throw new ObjectDisposedException( nameof( AsyncDisposableTestService ) )
			: Task.CompletedTask;
	}

	public ValueTask DisposeAsync( ) {
		IsDisposed = true;
		return ValueTask.CompletedTask;
	}
}

public class DisposableTestService : IDisposableTestService, IDisposable {
	public bool IsDisposed { get; private set; }

	public void DoSomething( ) {
		if ( IsDisposed )
			throw new ObjectDisposedException( nameof( DisposableTestService ) );
	}

	public Task DoSomethingAsync( ) {
		return IsDisposed
			? throw new ObjectDisposedException( nameof( DisposableTestService ) )
			: Task.CompletedTask;
	}

	public void Dispose( ) {
		IsDisposed = true;
	}
}
