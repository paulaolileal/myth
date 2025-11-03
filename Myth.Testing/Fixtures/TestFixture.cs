using Bogus;
using Microsoft.Extensions.DependencyInjection;

namespace Myth.Testing.Fixtures {

	/// <summary>
	/// A shared test fixture for managing test resources across multiple test classes
	/// </summary>
	/// <remarks>
	/// This fixture provides shared services and test data that can be reused across multiple test classes.
	/// Use this when you need to share expensive-to-create resources like database connections, web servers, etc.
	/// </remarks>
	public class TestFixture : IDisposable {
		private readonly IServiceCollection _services;
		private IServiceProvider? _serviceProvider;
		protected readonly Faker _faker;

		/// <summary>
		/// Gets the service provider for the test fixture
		/// </summary>
		public IServiceProvider ServiceProvider => _serviceProvider ?? throw new InvalidOperationException( "Services not built yet. Call BuildServices first." );

		/// <summary>
		/// Gets the faker instance for generating test data
		/// </summary>
		public Faker Faker => _faker;

		/// <summary>
		/// Initializes a new instance of the TestFixture class
		/// </summary>
		/// <param name="fakerCulture">The culture to use for Faker data generation</param>
		public TestFixture( string fakerCulture = "en_US" ) {
			_services = new ServiceCollection( );
			_faker = new Faker( fakerCulture );

			ConfigureServices( _services );
			BuildServices( );
		}

		/// <summary>
		/// Configure services for the test fixture
		/// </summary>
		/// <param name="services">The service collection to configure</param>
		/// <remarks>
		/// Override this method in derived classes to add custom services
		/// </remarks>
		protected virtual void ConfigureServices( IServiceCollection services ) {
			// Override in derived classes to add custom services
		}

		/// <summary>
		/// Build the service provider
		/// </summary>
		protected void BuildServices( ) {
			_serviceProvider = _services.BuildServiceProvider( );
		}

		/// <summary>
		/// Get a service from the test fixture
		/// </summary>
		/// <typeparam name="TService">The type of service</typeparam>
		/// <returns>The service instance or null if not found</returns>
		public TService? GetService<TService>( ) {
			return ServiceProvider.GetService<TService>( );
		}

		/// <summary>
		/// Get a required service from the test fixture
		/// </summary>
		/// <typeparam name="TService">The type of service</typeparam>
		/// <returns>The service instance</returns>
		/// <exception cref="InvalidOperationException">Thrown when the service is not found</exception>
		public TService GetRequiredService<TService>( ) where TService : notnull {
			return ServiceProvider.GetRequiredService<TService>( );
		}

		/// <summary>
		/// Dispose resources used by the test fixture
		/// </summary>
		public virtual void Dispose( ) {
			( _serviceProvider as IDisposable )?.Dispose( );
		}
	}
}