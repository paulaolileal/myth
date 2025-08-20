using Microsoft.Extensions.Options;
using Myth.DependencyInjection;
using Myth.Rest.Interfaces;

namespace Myth.Rest;

/// <summary>
/// Factory implementation for creating configured REST builders
/// </summary>
public class RestFactory : IRestFactory {
	private readonly IOptionsMonitor<RestFactorySettings> _options;
	private readonly Dictionary<string, Action<ConfigurationBuilder>> _configurations;

	public RestFactory( IOptionsMonitor<RestFactorySettings> options ) {
		_options = options ?? throw new ArgumentNullException( nameof( options ) );
		_configurations = new Dictionary<string, Action<ConfigurationBuilder>>( StringComparer.OrdinalIgnoreCase );

		// Load initial configurations
		RefreshConfigurations( );

		// Subscribe to configuration changes
		_options.OnChange( _ => RefreshConfigurations( ) );
	}

	private void RefreshConfigurations( ) {
		var currentOptions = _options.CurrentValue;
		_configurations.Clear( );

		foreach ( var config in currentOptions.Configurations ) {
			_configurations[ config.Key ] = config.Value;
		}
	}

	/// <summary>
	/// Create a REST builder with named configuration
	/// </summary>
	/// <param name="configurationName">The name of the configuration</param>
	/// <returns>A configured REST builder</returns>
	/// <exception cref="ArgumentException">Thrown when configuration name is not found</exception>
	public IRestRequest Create( string configurationName ) {
		if ( string.IsNullOrWhiteSpace( configurationName ) )
			throw new ArgumentException( "Configuration name cannot be null or empty", nameof( configurationName ) );

		if ( !_configurations.TryGetValue( configurationName, out var configuration ) )
			throw new ArgumentException( $"Configuration '{configurationName}' not found", nameof( configurationName ) );

		return Rest
			.Create( )
			.Configure( configuration );
	}

	/// <summary>
	/// Create a REST builder with custom configuration
	/// </summary>
	/// <param name="configurationBuilder">Custom configuration builder</param>
	/// <returns>A configured REST builder</returns>
	public IRestRequest Create( Action<ConfigurationBuilder> configurationBuilder ) {
		ArgumentNullException.ThrowIfNull( configurationBuilder );

		return Rest
			.Create( )
			.Configure( configurationBuilder );
	}
}