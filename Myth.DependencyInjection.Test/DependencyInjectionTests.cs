using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.DependencyInjection.Test.Interfaces;
using Myth.Extensions;
using Myth.ValueProviders;

namespace Myth.DependencyInjection.Test;

public class DependencyInjectionTests {

	[Fact]
	public void Application_namespace_should_return_string( ) {
		// Act
		var baseNamespace = TypeProvider.BaseApplicationNamespace;

		// Assert
		baseNamespace.Should( ).NotBeEmpty( );
		baseNamespace.Should( ).Be( "Myth" );
	}

	[Fact]
	public void Assemblies_should_return_a_list_of_assemblies( ) {
		// Act
		var assemblies = TypeProvider.ApplicationAssemblies;

		// Assert
		assemblies.Should( ).NotBeEmpty( );
	}

	[Fact]
	public void Types_should_return_a_list_of_Types( ) {
		// Act
		var types = TypeProvider.ApplicationTypes;

		// Assert
		types.Should( ).NotBeEmpty( );
	}

	[Fact]
	public void Types_assignabled_should_return_a_list_of_Types( ) {
		// Act
		var types = TypeProvider.GetTypesAssignableFrom<ITest>( );

		// Assert
		types.Should( ).NotBeEmpty( );
	}

	[Fact]
	public void Add_services_from_should_add_to_collection( ) {
		// Arrange
		var services = new ServiceCollection( );

		// Act
		services.AddServiceFromType<ITest>( );

		// Assert
		var result = services.First( );
		result!.ImplementationType!.Name.Should( ).Be( typeof( Models.Test ).Name );
		result.ServiceType.Name.Should( ).Be( typeof( ITest ).Name );
		result.Lifetime.Should( ).Be( ServiceLifetime.Scoped );
	}
}
