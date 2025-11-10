using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Extensions;
using Myth.Morph.Test.Models;
using Myth.Morph.Test.Models.Dtos;
using Myth.Morph.Test.Models.Entities;
using Myth.ServiceProvider;
using System.Reflection;
using System.Reflection.Emit;

namespace Myth.Morph.Test {

	/// <summary>
	/// Tests for handling Entity Framework proxies and inheritance scenarios.
	/// These tests ensure that the Morph library correctly resolves proxy types
	/// to their base types for both IMorphableTo and IMorphableFrom patterns.
	/// </summary>
	public class ProxyHandlingTests : IDisposable {
		private readonly IServiceProvider _serviceProvider;

		public ProxyHandlingTests( ) {
			MythServiceProvider.Reset( );
			var services = new ServiceCollection( );

			// Enable inheritance fallback to handle proxies properly
			services.AddMorph( settings => {
				settings.EnableInheritanceFallback = true;
				settings.MaxInheritanceDepth = 5;
				settings.IncludeInterfacesInFallback = false;
			} );

			services.AddLogging( );
			var serviceProvider = services.BuildServiceProvider( );
			MythServiceProvider.Initialize( serviceProvider );
			_serviceProvider = serviceProvider;
		}

		public void Dispose( ) {
			MythServiceProvider.Reset( );
		}

		/// <summary>
		/// Tests IMorphableFrom pattern with a simulated Entity Framework proxy as source.
		/// The proxy should be correctly resolved when the DTO implements IMorphableFrom<User>.
		/// </summary>
		[Fact]
		public void IMorphableFrom_WithProxy_Should_MapCorrectly( ) {
			// Arrange - Create a proxy that inherits from User
			var userProxy = CreateUserProxy( );

			// Act - SimpleUserDto implements IMorphableFrom<User>
			var dto = userProxy.To<SimpleUserDto>( _serviceProvider );

			// Assert
			dto.Should( ).NotBeNull( );
			dto.FirstName.Should( ).Be( "John" );
			dto.LastName.Should( ).Be( "Proxy" );
			dto.Email.Should( ).Be( "john@proxy.com" );
		}

		/// <summary>
		/// Tests that async bindings work correctly with proxy types in IMorphableFrom pattern.
		/// </summary>
		[Fact]
		public async Task IMorphableFrom_WithProxy_AsyncBindings_Should_Work( ) {
			// Arrange
			var userProxy = CreateUserProxy( );

			// Act - UserProfileDto implements IMorphableFrom<User> with service provider bindings
			var dto = await userProxy.ToAsync<UserProfileDto>( _serviceProvider );

			// Assert
			dto.Should( ).NotBeNull( );
			dto.FullName.Should( ).Be( "John Proxy" );
			dto.EmailStatus.Should( ).Be( "Verified" );
		}

		/// <summary>
		/// Tests that collection mapping works with proxies in the collection.
		/// </summary>
		[Fact]
		public void Collection_WithProxies_Should_MapAllItems( ) {
			// Arrange
			var proxies = new List<User> {
				CreateUserProxy( "Alice", "Smith", "alice@test.com" ),
				CreateUserProxy( "Bob", "Jones", "bob@test.com" ),
				CreateUserProxy( "Charlie", "Brown", "charlie@test.com" )
			};

			// Act
			var dtos = proxies.To<User, SimpleUserDto>( _serviceProvider ).ToList( );

			// Assert
			dtos.Should( ).HaveCount( 3 );
			dtos[ 0 ].FirstName.Should( ).Be( "Alice" );
			dtos[ 1 ].FirstName.Should( ).Be( "Bob" );
			dtos[ 2 ].FirstName.Should( ).Be( "Charlie" );
		}

		/// <summary>
		/// Tests that proxy types are handled correctly with complex bindings.
		/// </summary>
		[Fact]
		public void ProxyWithComplexBindings_Should_MapCorrectly( ) {
			// Arrange
			var productProxy = CreateProductProxy( );

			// Act - ProductSummaryDto implements IMorphableFrom<Product>
			var dto = productProxy.To<ProductSummaryDto>( _serviceProvider );

			// Assert
			dto.Should( ).NotBeNull( );
			dto.DisplayName.Should( ).Contain( "Proxy Product" );
			dto.FormattedPrice.Should( ).Be( "$99.99" );
		}

		/// <summary>
		/// Tests IMorphableTo with a proxy in inheritance hierarchy.
		/// </summary>
		[Fact]
		public void IMorphableTo_WithProxyInHierarchy_Should_ResolveBaseType( ) {
			// Arrange
			var derivedProxy = CreateDerivedEntityProxy( );

			// Act - DerivedEntity implements IMorphableTo<DerivedDto>
			var dto = derivedProxy.To<DerivedDto>( _serviceProvider );

			// Assert
			dto.Should( ).NotBeNull( );
			dto.BaseProperty.Should( ).Be( "Base Value" );
			dto.DerivedProperty.Should( ).Be( "Derived Value" );
		}

		/// <summary>
		/// Creates a dynamic proxy type that simulates Entity Framework proxy behavior.
		/// </summary>
		private User CreateUserProxy( string firstName = "John", string lastName = "Proxy", string email = "john@proxy.com" ) {
			var assemblyName = new AssemblyName( "DynamicProxyAssembly" );
			var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly( assemblyName, AssemblyBuilderAccess.Run );
			var moduleBuilder = assemblyBuilder.DefineDynamicModule( "DynamicProxyModule" );

			// Create proxy type with "Castle.Proxies" namespace to simulate EF proxy
			var typeBuilder = moduleBuilder.DefineType(
				"Castle.Proxies.UserProxy",
				TypeAttributes.Public | TypeAttributes.Class,
				typeof( User ) );

			var proxyType = typeBuilder.CreateType( );

			// Create instance and set properties
			var proxy = ( User )Activator.CreateInstance( proxyType )!;
			proxy.FirstName = firstName;
			proxy.LastName = lastName;
			proxy.Email = email;
			proxy.Id = 1;
			proxy.BirthDate = new DateTime( 1990, 1, 1 );
			proxy.CountryCode = "US";
			proxy.IsEmailVerified = true;
			proxy.LastLoginAt = DateTime.Now.AddDays( -3 );

			return proxy;
		}

		/// <summary>
		/// Creates a dynamic proxy for Product entity.
		/// </summary>
		private Product CreateProductProxy( ) {
			var assemblyName = new AssemblyName( "DynamicProxyAssembly" );
			var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly( assemblyName, AssemblyBuilderAccess.Run );
			var moduleBuilder = assemblyBuilder.DefineDynamicModule( "DynamicProxyModule" );

			var typeBuilder = moduleBuilder.DefineType(
				"Castle.Proxies.ProductProxy",
				TypeAttributes.Public | TypeAttributes.Class,
				typeof( Product ) );

			var proxyType = typeBuilder.CreateType( );

			var proxy = ( Product )Activator.CreateInstance( proxyType )!;
			proxy.Id = 1;
			proxy.Name = "Proxy Product";
			proxy.Price = 99.99m;
			proxy.CreatedAt = DateTime.Now.AddDays( -30 );
			proxy.IsActive = true;
			proxy.Category = "Electronics";
			proxy.Stock = 25;
			proxy.Weight = 1.5m;

			return proxy;
		}

		/// <summary>
		/// Creates a proxy for DerivedEntity that inherits from BaseEntity.
		/// </summary>
		private DerivedEntity CreateDerivedEntityProxy( ) {
			var assemblyName = new AssemblyName( "DynamicProxyAssembly" );
			var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly( assemblyName, AssemblyBuilderAccess.Run );
			var moduleBuilder = assemblyBuilder.DefineDynamicModule( "DynamicProxyModule" );

			var typeBuilder = moduleBuilder.DefineType(
				"Castle.Proxies.DerivedEntityProxy",
				TypeAttributes.Public | TypeAttributes.Class,
				typeof( DerivedEntity ) );

			var proxyType = typeBuilder.CreateType( );

			var proxy = ( DerivedEntity )Activator.CreateInstance( proxyType )!;
			proxy.BaseProperty = "Base Value";
			proxy.DerivedProperty = "Derived Value";

			return proxy;
		}
	}
}
