using Myth.Rest.Interfaces;

namespace Myth.Rest;

/// <summary>
/// Interface for REST factory that provides configured REST builders
/// </summary>
public interface IRestFactory {

	/// <summary>
	/// Create a REST builder with named configuration
	/// </summary>
	/// <param name="configurationName">The name of the configuration</param>
	/// <returns>A configured REST builder</returns>
	IRestRequest Create( string configurationName );

	/// <summary>
	/// Create a REST builder with custom configuration
	/// </summary>
	/// <param name="configurationBuilder">Custom configuration builder</param>
	/// <returns>A configured REST builder</returns>
	IRestRequest Create( Action<ConfigurationBuilder> configurationBuilder );
}