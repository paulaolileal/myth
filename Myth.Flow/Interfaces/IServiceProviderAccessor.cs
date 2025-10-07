namespace Myth.Interfaces {

	/// <summary>
	/// Provides access to an <see cref="IServiceProvider"/> instance for dependency injection.
	/// </summary>
	internal interface IServiceProviderAccessor {

		/// <summary>
		/// Gets the <see cref="IServiceProvider"/> instance used for dependency resolution.
		/// </summary>
		IServiceProvider ServiceProvider { get; }
	}
}