namespace Myth.Extensions {

	/// <summary>
	/// Provides a static service provider instance for the application.
	/// This class manages a single service provider that can be used throughout the application
	/// for dependency injection when no specific service provider is available.
	/// </summary>
	internal static class DefaultProvider {
		private static IServiceProvider? _serviceProvider;

		/// <summary>
		/// Gets or sets the application's service provider instance.
		/// </summary>
		/// <remarks>
		/// This property is typically used to configure or retrieve the application's dependency injection
		/// container. Ensure that the service provider is properly initialized before attempting to resolve
		/// services.
		/// </remarks>
		/// <value>The current service provider instance, or null if not configured.</value>
		public static IServiceProvider? ServiceProvider {
			get => _serviceProvider;
			set => _serviceProvider = value;
		}

		/// <summary>
		/// Ensures that a valid <see cref="IServiceProvider"/> is set for the application.
		/// </summary>
		/// <remarks>
		/// If the current service provider is not set and the provided <paramref name="sp"/> is not null,
		/// the method assigns the provided instance as the service provider.
		/// This method is thread-safe and will only set the provider if it's currently null.
		/// </remarks>
		/// <param name="sp">The <see cref="IServiceProvider"/> instance to set if no provider is currently configured.</param>
		public static void EnsureProvider( IServiceProvider? sp ) {
			if ( _serviceProvider is null && sp is not null )
				ServiceProvider = sp;
		}
	}
}