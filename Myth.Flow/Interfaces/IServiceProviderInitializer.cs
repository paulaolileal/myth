namespace Myth.Interfaces {

	/// <summary>
	/// Marker interface for initializing the <see cref="IServiceProvider"/> in Myth.Flow.
	/// Used to trigger global service provider setup in the dependency injection container.
	/// </summary>
	public interface IServiceProviderInitializer { }
}