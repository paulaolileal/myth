using Myth.Interfaces;

namespace Myth.Builders {

	/// <summary>
	/// Provides access to an <see cref="IServiceProvider"/> instance for dependency injection.
	/// Implements <see cref="IServiceProviderAccessor"/>.
	/// </summary>
	internal sealed class ServiceProviderAccessor : IServiceProviderAccessor {

		/// <summary>
		/// Gets the <see cref="IServiceProvider"/> instance used for dependency resolution.
		/// </summary>
		public IServiceProvider ServiceProvider { get; }

		/// <summary>
		/// Initializes a new instance of <see cref="ServiceProviderAccessor"/> with the specified <see cref="IServiceProvider"/>.
		/// </summary>
		/// <param name="serviceProvider">The service provider to be accessed.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceProvider"/> is null.</exception>
		public ServiceProviderAccessor( IServiceProvider serviceProvider ) =>
			ServiceProvider = serviceProvider ?? throw new ArgumentNullException( nameof( serviceProvider ) );
	}
}