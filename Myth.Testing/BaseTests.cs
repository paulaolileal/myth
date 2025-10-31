using Bogus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Myth.Testing.Mocks;

namespace Myth.Testing {

	/// <summary>
	/// Base for unit tests
	/// </summary>
	/// <remarks>
	/// A service collection is created and a configuration service
	/// </remarks>
	public abstract class BaseTests : IDisposable {
		protected IServiceProvider _serviceProvider = null!;
		protected readonly IServiceCollection _services;
		protected readonly Faker _faker;

		private IDictionary<string, string> _configurationValues = new Dictionary<string, string> {
			{"Test", "Test"},
			{"Test:TestChild", "TestChild"}
		};

		public BaseTests( string fakerCulture = "en_US" ) {
			_services = new ServiceCollection( );
			_faker = new Faker( fakerCulture );

			Setup( );
		}

		/// <summary>
		/// Setup the environment for run the test
		/// </summary>
		/// <remarks>
		/// The following services are injected to application service collection:
		/// <list type="bullet">
		///     <item>
		///         <term>ILogger</term>
		///         <description>Mock logging service</description>
		///     </item>
		///     <item>
		///         <term>IHostEnvironment</term>
		///         <description>Mock host environment service</description>
		///     </item>
		///     <item>
		///         <term>IConfiguration</term>
		///         <description>The configuration service</description>
		///     </item>
		/// </list>
		/// </remarks>
		private void Setup( ) {
			_services.AddSingleton<ILogger>( LoggingMock.Logger );
			_services.AddSingleton<IHostEnvironment, HostEnvironmentMock>( );

			AddConfiguration( );

			_serviceProvider = _services.BuildServiceProvider( );
		}

		/// <summary>
		/// Add configuration service
		/// </summary>
		/// <param name="replace">Force to replace the service in service collection</param>
		/// <remarks>
		/// Use the <paramref name="replace"/> when the <see cref="AddConfigurationItem(string, string)"/> is called
		/// </remarks>
		public virtual void AddConfiguration( bool replace = false ) {
			var configuration = new ConfigurationBuilder( )
				.AddInMemoryCollection( _configurationValues! )
				.Build( );

			var service = new ServiceDescriptor( typeof( IConfiguration ), configuration );
			if ( !replace )
				_services.AddSingleton<IConfiguration>( configuration );
			else
				_services.Replace( service );
		}

		/// <summary>
		/// Add a item to configuration file
		/// </summary>
		/// <param name="key">The key of item</param>
		/// <param name="value">The value of item</param>
		/// <remarks>
		/// Exemple:
		/// <para>"Key": "TopLevel:Children",</para>
		/// <para>"Key2": "value"</para>
		/// </remarks>
		public void AddConfigurationItem( string key, string value ) {
			_configurationValues.Add( key, value );

			AddConfiguration( true );
		}

		/// <summary>
		/// Add a service to the tests service collection
		/// </summary>
		/// <typeparam name="TInterface">The type of service</typeparam>
		/// <param name="instance">The instance of type</param>
		/// <param name="reBuildOnAdd">Re-build the provider</param>
		/// <remarks>
		/// Use the rebuild when a service was added after the constructor
		/// </remarks>
		public virtual void AddService<TInterface>( TInterface instance, bool reBuildOnAdd = true ) {
			if ( instance is null )
				throw new ArgumentNullException( nameof( instance ) );

			var service = new ServiceDescriptor( typeof( TInterface ), instance );

			_services.Add( service );

			if ( reBuildOnAdd )
				_serviceProvider = _services.BuildServiceProvider( );
		}

		/// <summary>
		/// Add a service to the tests service collection
		/// </summary>
		/// <typeparam name="TInterface">The type of service</typeparam>
		/// <typeparam name="TClass">The instance of service</typeparam>
		/// <param name="lifetime">The service lifetime: Scoped | Singleton | Transient</param>
		/// <param name="reBuildOnAdd">Re-build the provider</param>
		/// <remarks>
		/// Use the rebuild when a service was added after the constructor
		/// </remarks>
		public virtual void AddService<TInterface, TClass>( ServiceLifetime lifetime = ServiceLifetime.Scoped, bool reBuildOnAdd = true ) where TClass : TInterface {
			var service = new ServiceDescriptor( typeof( TInterface ), typeof( TClass ), lifetime );

			_services.Add( service );

			if ( reBuildOnAdd )
				_serviceProvider = _services.BuildServiceProvider( );
		}

		/// <summary>
		/// Adds services to the test service collection using a custom action.
		/// </summary>
		/// <param name="services">An action that receives the current service collection for configuration.</param>
		public virtual void AddServices( Action<IServiceCollection> services ) {
			services.Invoke( _services );

			_serviceProvider = _services.BuildServiceProvider( );
		}

		/// <summary>
		/// Get a service from test service collection
		/// </summary>
		/// <typeparam name="TService">The type of service</typeparam>
		/// <returns>The instance of service</returns>
		/// <remarks>The result can be null if the service not exists</remarks>
		public virtual TService? GetService<TService>( ) {
			return _serviceProvider.GetService<TService>( );
		}

		/// <summary>
		/// Get a service from test service collection
		/// </summary>
		/// <typeparam name="TService">The type of service</typeparam>
		/// <returns>The instance of service</returns>
		public virtual TService GetRequiredService<TService>( ) where TService : notnull {
			return _serviceProvider.GetRequiredService<TService>( );
		}

		/// <summary>
		/// Create a new instance of the type informed getting the arguments services from test service collection
		/// </summary>
		/// <typeparam name="T">The type to create</typeparam>
		/// <param name="args">Additional constructor arguments</param>
		/// <returns>The instance of type</returns>
		public T CreateInstance<T>( params object[ ] args ) {
			return ActivatorUtilities.CreateInstance<T>( _serviceProvider, args );
		}

		/// <summary>
		/// Add multiple services using configuration action
		/// </summary>
		/// <param name="configure">Configuration action for services</param>
		/// <param name="reBuildOnAdd">Whether to rebuild the service provider</param>
		public virtual void ConfigureServices( Action<IServiceCollection> configure, bool reBuildOnAdd = true ) {
			configure?.Invoke( _services );

			if ( reBuildOnAdd )
				_serviceProvider = _services.BuildServiceProvider( );
		}

		/// <summary>
		/// Replace an existing service registration
		/// </summary>
		/// <typeparam name="TInterface">The service interface type</typeparam>
		/// <param name="implementation">The new implementation instance</param>
		/// <param name="reBuildOnAdd">Whether to rebuild the service provider</param>
		public virtual void ReplaceService<TInterface>( TInterface implementation, bool reBuildOnAdd = true ) where TInterface : class {
			if ( implementation is null )
				throw new ArgumentNullException( nameof( implementation ) );

			// Remove existing registrations
			var existingDescriptors = _services.Where( s => s.ServiceType == typeof( TInterface ) ).ToList( );
			foreach ( var descriptor in existingDescriptors )
				_services.Remove( descriptor );

			// Add new registration
			_services.AddSingleton( implementation );

			if ( reBuildOnAdd )
				_serviceProvider = _services.BuildServiceProvider( );
		}

		/// <summary>
		/// Check if a service is registered
		/// </summary>
		/// <typeparam name="TService">The service type</typeparam>
		/// <returns>True if the service is registered, false otherwise</returns>
		public virtual bool IsServiceRegistered<TService>( ) {
			return _services.Any( s => s.ServiceType == typeof( TService ) );
		}

		/// <summary>
		/// Create a scoped service provider for scoped tests
		/// </summary>
		/// <returns>A disposable scoped service provider</returns>
		public virtual IServiceScope CreateScope( ) {
			return _serviceProvider.CreateScope( );
		}

		/// <summary>
		/// Dispose resources used by the test
		/// </summary>
		public virtual void Dispose( ) {
			( _serviceProvider as IDisposable )?.Dispose( );
		}
	}
}