using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Extensions;
using Myth.ServiceProvider;

namespace Myth.Commons.Test;

public class ScopedServiceTests {

	[Fact]
	public void AddScopedServiceProvider_ShouldRegisterIScopedService( ) {
		// Arrange
		var services = new ServiceCollection( );

		// Act
		services.AddScopedServiceProvider( );
		var provider = services.BuildServiceProvider( );

		// Assert
		var scopedService = provider.GetService<IScopedService<ITestService>>( );
		scopedService.Should( ).NotBeNull( );
	}

	[Fact]
	public void ScopedService_Execute_ShouldResolveServiceAndExecuteOperation( ) {
		// Arrange
		var services = new ServiceCollection( );
		services.AddScopedServiceProvider( );
		services.AddScoped<ITestService, TestService>( );

		var provider = services.BuildServiceProvider( );
		var scopedService = provider.GetRequiredService<IScopedService<ITestService>>( );

		// Act
		var result = scopedService.Execute( service => service.GetValue( ) );

		// Assert
		result.Should( ).Be( "TestValue" );
	}

	[Fact]
	public async Task ScopedService_ExecuteAsync_ShouldResolveServiceAndExecuteOperation( ) {
		// Arrange
		var services = new ServiceCollection( );
		services.AddScopedServiceProvider( );
		services.AddScoped<ITestService, TestService>( );

		var provider = services.BuildServiceProvider( );
		var scopedService = provider.GetRequiredService<IScopedService<ITestService>>( );

		// Act
		var result = await scopedService.ExecuteAsync( service => service.GetValueAsync( ) );

		// Assert
		result.Should( ).Be( "AsyncTestValue" );
	}

	[Fact]
	public void ScopedService_Execute_VoidOperation_ShouldExecuteSuccessfully( ) {
		// Arrange
		var services = new ServiceCollection( );
		services.AddScopedServiceProvider( );
		services.AddScoped<ITestService, TestService>( );

		var provider = services.BuildServiceProvider( );
		var scopedService = provider.GetRequiredService<IScopedService<ITestService>>( );

		var executed = false;

		// Act
		scopedService.Execute( service => {
			service.DoSomething( );
			executed = true;
		} );

		// Assert
		executed.Should( ).BeTrue( );
	}

	[Fact]
	public async Task ScopedService_ExecuteAsync_VoidOperation_ShouldExecuteSuccessfully( ) {
		// Arrange
		var services = new ServiceCollection( );
		services.AddScopedServiceProvider( );
		services.AddScoped<ITestService, TestService>( );

		var provider = services.BuildServiceProvider( );
		var scopedService = provider.GetRequiredService<IScopedService<ITestService>>( );

		var executed = false;

		// Act
		await scopedService.ExecuteAsync( async service => {
			await service.DoSomethingAsync( );
			executed = true;
		} );

		// Assert
		executed.Should( ).BeTrue( );
	}

	[Fact]
	public void ScopedService_Execute_WithUnregisteredService_ShouldThrowInvalidOperationException( ) {
		// Arrange
		var services = new ServiceCollection( );
		services.AddScopedServiceProvider( );
		// Não registra ITestService intencionalmente

		var provider = services.BuildServiceProvider( );
		var scopedService = provider.GetRequiredService<IScopedService<ITestService>>( );

		// Act
		var act = ( ) => scopedService.Execute( service => service.GetValue( ) );

		// Assert
		act.Should( ).Throw<InvalidOperationException>( )
		   .WithMessage( "*Service of type*ITestService*could not be resolved*" );
	}

	[Fact]
	public void ScopedService_Execute_WithNullOperation_ShouldThrowArgumentNullException( ) {
		// Arrange
		var services = new ServiceCollection( );
		services.AddScopedServiceProvider( );
		services.AddScoped<ITestService, TestService>( );

		var provider = services.BuildServiceProvider( );
		var scopedService = provider.GetRequiredService<IScopedService<ITestService>>( );

		// Act
		var act = ( ) => scopedService.Execute<string>( null! );

		// Assert
		act.Should( ).Throw<ArgumentNullException>( );
	}

	[Fact]
	public void ScopedService_WorksWithDifferentLifetimes( ) {
		// Arrange
		var services = new ServiceCollection( );
		services.AddScopedServiceProvider( );
		services.AddSingleton<ISingletonService, SingletonService>( );
		services.AddTransient<ITransientService, TransientService>( );

		var provider = services.BuildServiceProvider( );

		// Act & Assert - Singleton service
		var singletonScopedService = provider.GetRequiredService<IScopedService<ISingletonService>>( );
		var singletonResult = singletonScopedService.Execute( service => service.GetValue( ) );
		singletonResult.Should( ).Be( "Singleton" );

		// Act & Assert - Transient service
		var transientScopedService = provider.GetRequiredService<IScopedService<ITransientService>>( );
		var transientResult = transientScopedService.Execute( service => service.GetValue( ) );
		transientResult.Should( ).Be( "Transient" );
	}

	[Fact]
	public void AddScopedServiceProvider_WithNullServices_ShouldThrowArgumentNullException( ) {
		// Arrange
		IServiceCollection services = null!;

		// Act
		var act = ( ) => services.AddScopedServiceProvider( );

		// Assert
		act.Should( ).Throw<ArgumentNullException>( );
	}

	// Test interfaces and implementations
	public interface ITestService {

		string GetValue( );

		Task<string> GetValueAsync( );

		void DoSomething( );

		Task DoSomethingAsync( );
	}

	public class TestService : ITestService {

		public string GetValue( ) => "TestValue";

		public Task<string> GetValueAsync( ) => Task.FromResult( "AsyncTestValue" );

		public void DoSomething( ) {
		}

		public Task DoSomethingAsync( ) => Task.CompletedTask;
	}

	public interface ISingletonService {

		string GetValue( );
	}

	public class SingletonService : ISingletonService {

		public string GetValue( ) => "Singleton";
	}

	public interface ITransientService {

		string GetValue( );
	}

	public class TransientService : ITransientService {

		public string GetValue( ) => "Transient";
	}
}