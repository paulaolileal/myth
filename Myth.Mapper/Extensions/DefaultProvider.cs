namespace Myth.Extensions {

	internal static class DefaultProvider {
		private static IServiceProvider? _serviceProvider;

		/// <summary>
		/// Gets or sets the application's service provider instance.
		/// </summary>
		/// <remarks>This property is typically used to configure or retrieve the application's dependency injection
		/// container. Ensure that the service provider is properly initialized before attempting to resolve
		/// services.
		/// </remarks>
		public static IServiceProvider? ServiceProvider {
			get => _serviceProvider;
			set => _serviceProvider = value;
		}

		/// <summary>
		/// Ensures that a valid <see cref="IServiceProvider"/> is set for the application.
		/// </summary>
		/// <remarks>If the current service provider is not set and the provided <paramref name="sp"/> is setted
		/// the method assigns the provided instance as the service provider.
		/// </remarks>
		/// <param name="sp">The <see cref="IServiceProvider"/> instance to set if no provider is currently configured.
		public static void EnsureProvider( IServiceProvider? sp ) {
			if ( _serviceProvider is null && sp is not null )
				ServiceProvider = sp;
		}
	}
}