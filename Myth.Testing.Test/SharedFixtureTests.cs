using Microsoft.Extensions.DependencyInjection;
using Myth.Testing.Fixtures;
using Myth.Testing.Test.Services;

namespace Myth.Testing.Test;

/// <summary>
/// Example fixture for sharing services across multiple test classes
/// </summary>
public class SharedServiceFixture : TestFixture {

	/// <summary>
	/// Configure shared services
	/// </summary>
	/// <param name="services">Service collection</param>
	protected override void ConfigureServices( IServiceCollection services ) {
		// Add shared services that are expensive to create
		services.AddSingleton<UserService>( );
		services.AddSingleton<ExpensiveService>( );

		base.ConfigureServices( services );
	}
}
