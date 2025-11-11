using Myth.Builders;

namespace Myth.Interfaces;

/// <summary>
/// Interface for configuring REST client settings
/// </summary>
public interface IRestBuilder {

	/// <summary>
	/// Configure REST client settings
	/// </summary>
	/// <param name="configurationBuilder">Configuration action</param>
	/// <returns>REST actions interface</returns>
	IRestRequest Configure( Action<ConfigurationBuilder> configurationBuilder );
}
