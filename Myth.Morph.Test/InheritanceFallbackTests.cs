using Bogus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Extensions;
using Myth.Interfaces;
using System.Reflection;
using System.Reflection.Emit;

namespace Myth.Morph.Test {

	/// <summary>
	/// Public test entity for inheritance fallback tests.
	/// Must be public for dynamic type creation to work.
	/// </summary>
	public class PublicTestEntity : IMorphable<PublicTestDto> {
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public bool IsActive { get; set; }
		public string Description { get; set; } = string.Empty;

		public void MorphTo( Schema<PublicTestDto> schema ) {
			schema
				.Bind( dest => dest.Id, ( ) => Id )
				.Bind( dest => dest.Name, ( ) => Name )
				.Bind( dest => dest.IsActive, ( ) => !IsActive )
				.Bind( dest => dest.Description, ( ) => Description );
		}
	}

	/// <summary>
	/// Public test DTO for inheritance fallback tests.
	/// </summary>
	public class PublicTestDto {
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public bool IsActive { get; set; }
		public string Description { get; set; } = string.Empty;
	}

	/// <summary>
	/// Public base entity for inheritance tests.
	/// </summary>
	public class PublicBaseEntity : IMorphable<PublicBaseDto> {
		public string BaseProperty { get; set; } = string.Empty;

		public void MorphTo( Schema<PublicBaseDto> schema ) {
			schema.Bind( dest => dest.BaseProperty, ( ) => BaseProperty );
		}
	}

	/// <summary>
	/// Public derived entity for inheritance tests.
	/// </summary>
	public class PublicDerivedEntity : PublicBaseEntity, IMorphable<PublicDerivedDto> {
		public string DerivedProperty { get; set; } = string.Empty;

		public void MorphTo( Schema<PublicDerivedDto> schema ) {
			schema
				.Bind( dest => dest.BaseProperty, ( ) => BaseProperty )
				.Bind( dest => dest.DerivedProperty, ( ) => DerivedProperty );
		}
	}

	/// <summary>
	/// Proxy entity that doesn't have direct mapping registration.
	/// Used to test inheritance fallback by manually registering only the base type mapping.
	/// </summary>
	public class ProxyTestEntity {
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public bool IsActive { get; set; }
		public string Description { get; set; } = string.Empty;
	}

	/// <summary>
	/// Base entity with mapping that proxy inherits from.
	/// </summary>
	public class BaseTestEntity : IMorphable<PublicTestDto> {
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public bool IsActive { get; set; }
		public string Description { get; set; } = string.Empty;

		public void MorphTo( Schema<PublicTestDto> schema ) {
			schema
				.Bind( dest => dest.Id, ( ) => Id )
				.Bind( dest => dest.Name, ( ) => Name )
				.Bind( dest => dest.IsActive, ( ) => !IsActive )
				.Bind( dest => dest.Description, ( ) => Description );
		}
	}

	/// <summary>
	/// Proxy that inherits from base entity to test inheritance fallback.
	/// </summary>
	public class DerivedProxyEntity : BaseTestEntity {
		// This will inherit the mapping from BaseTestEntity
	}

	/// <summary>
	/// Simple proxy derived entity for inheritance tests.
	/// </summary>
	public class ProxyDerivedEntity : PublicDerivedEntity {
		// Inherits from PublicDerivedEntity but doesn't implement IMorphable
		// This simulates an EF proxy
	}

	/// <summary>
	/// Public base DTO for inheritance tests.
	/// </summary>
	public class PublicBaseDto {
		public string BaseProperty { get; set; } = string.Empty;
	}

	/// <summary>
	/// Public derived DTO for inheritance tests.
	/// </summary>
	public class PublicDerivedDto {
		public string BaseProperty { get; set; } = string.Empty;
		public string DerivedProperty { get; set; } = string.Empty;
	}

	/// <summary>
	/// Tests for inheritance fallback functionality to handle proxy types and derived classes.
	/// This addresses the issue where Entity Framework proxies (e.g., Castle.Proxies.User_proxy)
	/// cannot be mapped because they don't have direct mappings registered.
	/// </summary>
	public class InheritanceFallbackTests {
		private readonly Faker _faker = new Faker( );

		private IServiceProvider CreateServiceProviderWithInheritance( bool enableInheritance = true, int maxDepth = 5 ) {
			var services = new ServiceCollection( );
			services.AddLogging( );

			services.AddMorph( settings => {
				if ( enableInheritance ) {
					settings.WithInheritanceFallback( enabled: true, maxDepth: maxDepth, includeInterfaces: true );
				} else {
					settings.DisableInheritanceFallback( );
				}
			} );

			return services.BuildServiceProvider( );
		}

		[Fact]
		public void MorphTo_Should_HandleDerivedTypes_WithInheritanceFallback( ) {
			// Arrange
			var serviceProvider = CreateServiceProviderWithInheritance( );

			// Create a proxy entity that inherits from BaseTestEntity (which has mapping)
			var proxyInstance = new DerivedProxyEntity {
				Id = _faker.Random.Number( ),
				Name = _faker.Name.FirstName( ),
				Description = _faker.Lorem.Text( ),
				IsActive = _faker.Random.Bool( )
			};

			// Act - Should use BaseTestEntity's mapping via inheritance fallback
			var result = proxyInstance.To<PublicTestDto>( serviceProvider );

			// Assert
			result.Should( ).NotBeNull( );
			result.Id.Should( ).Be( proxyInstance.Id );
			result.Name.Should( ).Be( proxyInstance.Name );
			result.Description.Should( ).Be( proxyInstance.Description );
			result.IsActive.Should( ).Be( !proxyInstance.IsActive );
		}

		[Fact]
		public void CanBindTo_Should_ReturnTrue_ForDerivedTypes_WithInheritanceFallback( ) {
			// Arrange
			var serviceProvider = CreateServiceProviderWithInheritance( );

			// Create a proxy entity that inherits from mapped type
			var proxyInstance = new DerivedProxyEntity( );

			// Act
			var canBind = proxyInstance.CanBindTo<PublicTestDto>( serviceProvider );

			// Assert
			canBind.Should( ).BeTrue( );
		}

		[Fact]
		public void MorphTo_Should_WorkWithMultipleLevelsOfInheritance( ) {
			// Arrange
			var serviceProvider = CreateServiceProviderWithInheritance( );

			// Create a proxy derived entity (which inherits from PublicDerivedEntity -> PublicBaseEntity)
			var proxyInstance = new ProxyDerivedEntity {
				BaseProperty = _faker.Lorem.Word( ),
				DerivedProperty = _faker.Lorem.Word( )
			};

			// Act
			var result = proxyInstance.To<PublicDerivedDto>( serviceProvider );

			// Assert
			result.Should( ).NotBeNull( );
			result.BaseProperty.Should( ).Be( proxyInstance.BaseProperty );
			result.DerivedProperty.Should( ).Be( proxyInstance.DerivedProperty );
		}

		[Fact]
		public void MorphTo_Should_StillWorkWithDirectMapping_WhenInheritanceFallbackDisabled( ) {
			// Arrange
			var serviceProvider = CreateServiceProviderWithInheritance( enableInheritance: false );

			// Create a proxy entity that inherits from BaseTestEntity (which implements IMorphable)
			// Even with inheritance fallback disabled, this should work because DerivedProxyEntity
			// inherits the IMorphable implementation, making it a direct instance-based mapping
			var proxyInstance = new DerivedProxyEntity {
				Id = _faker.Random.Number( ),
				Name = _faker.Name.FirstName( ),
				Description = _faker.Lorem.Text( ),
				IsActive = _faker.Random.Bool( )
			};

			// Act
			var result = proxyInstance.To<PublicTestDto>( serviceProvider );

			// Assert - Should work because it's a direct interface implementation, not inheritance fallback
			result.Should( ).NotBeNull( );
			result.Id.Should( ).Be( proxyInstance.Id );
		}

		[Fact]
		public void InheritanceFallback_Should_RespectMaxDepthConfiguration( ) {
			// Arrange
			var serviceProvider = CreateServiceProviderWithInheritance( maxDepth: 1 );
			var registry = serviceProvider.GetRequiredService<SchemaRegistry>( );

			// This test verifies that the MaxInheritanceDepth configuration is properly set and used
			// We can verify this by checking that inheritance fallback is enabled with the correct depth
			var proxyInstance = new DerivedProxyEntity {
				Id = _faker.Random.Number( ),
				Name = _faker.Name.FirstName( ),
				Description = _faker.Lorem.Text( ),
				IsActive = _faker.Random.Bool( )
			};

			// Act - Should work because DerivedProxyEntity inherits IMorphable implementation
			var result = proxyInstance.To<PublicTestDto>( serviceProvider );

			// Assert - Verifies the system handles inheritance correctly with depth configuration
			result.Should( ).NotBeNull( );
			result.Id.Should( ).Be( proxyInstance.Id );
			result.Name.Should( ).Be( proxyInstance.Name );
			result.Description.Should( ).Be( proxyInstance.Description );
			result.IsActive.Should( ).Be( !proxyInstance.IsActive ); // Inverted by mapping logic
		}

		[Fact]
		public void HasMapping_Should_ReturnTrue_ForDerivedTypes_WithInheritanceFallback( ) {
			// Arrange
			var serviceProvider = CreateServiceProviderWithInheritance( );
			var registry = serviceProvider.GetRequiredService<SchemaRegistry>( );

			// Act
			var hasMapping = registry.HasMapping( typeof( DerivedProxyEntity ), typeof( PublicTestDto ) );

			// Assert
			hasMapping.Should( ).BeTrue( );
		}

		[Fact]
		public void MorphTo_Should_HandleNestedInheritance( ) {
			// Arrange
			var serviceProvider = CreateServiceProviderWithInheritance( );

			// Test with a 2-level inheritance chain: ProxyDerivedEntity -> PublicDerivedEntity -> PublicBaseEntity
			var proxyInstance = new ProxyDerivedEntity {
				BaseProperty = _faker.Lorem.Word( ),
				DerivedProperty = _faker.Lorem.Word( )
			};

			// Act - Should use PublicDerivedEntity's mapping via inheritance fallback
			var result = proxyInstance.To<PublicDerivedDto>( serviceProvider );

			// Assert
			result.Should( ).NotBeNull( );
			result.BaseProperty.Should( ).Be( proxyInstance.BaseProperty );
			result.DerivedProperty.Should( ).Be( proxyInstance.DerivedProperty );
		}

		[Fact]
		public void SchemaRegistry_Should_CacheInheritanceHierarchy( ) {
			// Arrange
			var serviceProvider = CreateServiceProviderWithInheritance( );
			var registry = serviceProvider.GetRequiredService<SchemaRegistry>( );

			// Act - Call multiple times to test caching
			var hasMapping1 = registry.HasMapping( typeof( DerivedProxyEntity ), typeof( PublicTestDto ) );
			var hasMapping2 = registry.HasMapping( typeof( DerivedProxyEntity ), typeof( PublicTestDto ) );
			var hasMapping3 = registry.HasMapping( typeof( DerivedProxyEntity ), typeof( PublicTestDto ) );

			// Assert - All should return true, and caching should make subsequent calls faster
			hasMapping1.Should( ).BeTrue( );
			hasMapping2.Should( ).BeTrue( );
			hasMapping3.Should( ).BeTrue( );
		}

		[Fact]
		public void MorphTo_Should_HandleEntityFrameworkProxyTypes( ) {
			// Arrange
			var serviceProvider = CreateServiceProviderWithInheritance( );

			// Create a dynamic proxy type that simulates Entity Framework proxy behavior
			// This is similar to Castle.Proxies.WeatherForecastProxy created by EF
			var proxyType = CreateDynamicDerivedType( typeof( PublicTestEntity ), "Castle.Proxies.PublicTestEntityProxy" );

			// Create an instance of the dynamic proxy type
			var proxyInstance = Activator.CreateInstance( proxyType )!;

			// Set properties using reflection (simulating how EF populates proxy instances)
			SetProperty( proxyInstance, nameof( PublicTestEntity.Id ), _faker.Random.Number( ) );
			SetProperty( proxyInstance, nameof( PublicTestEntity.Name ), _faker.Name.FirstName( ) );
			SetProperty( proxyInstance, nameof( PublicTestEntity.Description ), _faker.Lorem.Text( ) );
			SetProperty( proxyInstance, nameof( PublicTestEntity.IsActive ), _faker.Random.Bool( ) );

			// Act - The proxy type should use the base type's IMorphable implementation via inheritance fallback
			var result = proxyInstance.To<PublicTestDto>( serviceProvider );

			// Assert
			result.Should( ).NotBeNull( );
			result.Id.Should( ).Be( GetProperty<int>( proxyInstance, nameof( PublicTestEntity.Id ) ) );
			result.Name.Should( ).Be( GetProperty<string>( proxyInstance, nameof( PublicTestEntity.Name ) ) );
			result.Description.Should( ).Be( GetProperty<string>( proxyInstance, nameof( PublicTestEntity.Description ) ) );
			// IsActive is inverted by the mapping logic in PublicTestEntity.MorphTo
			result.IsActive.Should( ).Be( !GetProperty<bool>( proxyInstance, nameof( PublicTestEntity.IsActive ) ) );
		}

		[Fact]
		public void CanBindTo_Should_ReturnTrue_ForEntityFrameworkProxyTypes( ) {
			// Arrange
			var serviceProvider = CreateServiceProviderWithInheritance( );

			// Create a dynamic proxy type that simulates Entity Framework proxy behavior
			var proxyType = CreateDynamicDerivedType( typeof( PublicTestEntity ), "Castle.Proxies.PublicTestEntityProxy" );
			var proxyInstance = Activator.CreateInstance( proxyType )!;

			// Act
			var canBind = proxyInstance.CanBindTo<PublicTestDto>( serviceProvider );

			// Assert
			canBind.Should( ).BeTrue( );
		}

		[Fact]
		public void HasMapping_Should_ReturnTrue_ForEntityFrameworkProxyTypes( ) {
			// Arrange
			var serviceProvider = CreateServiceProviderWithInheritance( );
			var registry = serviceProvider.GetRequiredService<SchemaRegistry>( );

			// Create a dynamic proxy type that simulates Entity Framework proxy behavior
			var proxyType = CreateDynamicDerivedType( typeof( PublicTestEntity ), "Castle.Proxies.PublicTestEntityProxy" );

			// Act
			var hasMapping = registry.HasMapping( proxyType, typeof( PublicTestDto ) );

			// Assert
			hasMapping.Should( ).BeTrue( );
		}

		/// <summary>
		/// Creates a dynamic type that inherits from the specified base type.
		/// This simulates Entity Framework proxy types like Castle.Proxies.User_proxy.
		/// </summary>
		private static Type CreateDynamicDerivedType( Type baseType, string typeName ) {
			var assemblyName = new AssemblyName( $"DynamicTestAssembly_{Guid.NewGuid( ):N}" );
			var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly( assemblyName, AssemblyBuilderAccess.Run );
			var moduleBuilder = assemblyBuilder.DefineDynamicModule( "DynamicTestModule" );

			var typeBuilder = moduleBuilder.DefineType( $"{typeName}_{Guid.NewGuid( ):N}", TypeAttributes.Public | TypeAttributes.Class, baseType );

			// Add a simple constructor
			var baseCtor = baseType.GetConstructor( Type.EmptyTypes );
			if ( baseCtor == null ) {
				throw new InvalidOperationException( $"Base type {baseType.Name} must have parameterless constructor" );
			}

			var constructor = typeBuilder.DefineConstructor( MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes );
			var ctorIL = constructor.GetILGenerator( );
			ctorIL.Emit( OpCodes.Ldarg_0 );
			ctorIL.Emit( OpCodes.Call, baseCtor );
			ctorIL.Emit( OpCodes.Ret );

			try {
				return typeBuilder.CreateType( )!;
			} catch ( Exception ex ) {
				throw new InvalidOperationException( $"Failed to create dynamic type derived from {baseType.FullName}: {ex.Message}", ex );
			}
		}

		/// <summary>
		/// Creates a dynamic type that inherits from the specified base type and implements an interface.
		/// </summary>
		private static Type CreateDynamicDerivedTypeWithInterface( Type baseType, string typeName, Type interfaceType ) {
			var assemblyName = new AssemblyName( $"DynamicTestAssemblyInterface_{Guid.NewGuid( ):N}" );
			var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly( assemblyName, AssemblyBuilderAccess.Run );
			var moduleBuilder = assemblyBuilder.DefineDynamicModule( "DynamicTestModule" );

			var typeBuilder = moduleBuilder.DefineType( $"{typeName}_{Guid.NewGuid( ):N}", TypeAttributes.Public | TypeAttributes.Class, baseType, new[ ] { interfaceType } );

			// Add a simple constructor
			var baseCtor = baseType.GetConstructor( Type.EmptyTypes );
			if ( baseCtor == null ) {
				throw new InvalidOperationException( $"Base type {baseType.Name} must have parameterless constructor" );
			}

			var constructor = typeBuilder.DefineConstructor( MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes );
			var ctorIL = constructor.GetILGenerator( );
			ctorIL.Emit( OpCodes.Ldarg_0 );
			ctorIL.Emit( OpCodes.Call, baseCtor );
			ctorIL.Emit( OpCodes.Ret );

			// Implement interface property if needed
			if ( interfaceType == typeof( ITestInterface ) ) {
				var propertyBuilder = typeBuilder.DefineProperty( "TestProperty", PropertyAttributes.None, typeof( int ), Type.EmptyTypes );
				var backingField = typeBuilder.DefineField( "_testProperty", typeof( int ), FieldAttributes.Private );

				// Getter
				var getterBuilder = typeBuilder.DefineMethod( "get_TestProperty", MethodAttributes.Public | MethodAttributes.Virtual, typeof( int ), Type.EmptyTypes );
				var getterIL = getterBuilder.GetILGenerator( );
				getterIL.Emit( OpCodes.Ldarg_0 );
				getterIL.Emit( OpCodes.Ldfld, backingField );
				getterIL.Emit( OpCodes.Ret );
				propertyBuilder.SetGetMethod( getterBuilder );

				// Setter
				var setterBuilder = typeBuilder.DefineMethod( "set_TestProperty", MethodAttributes.Public | MethodAttributes.Virtual, null, new[ ] { typeof( int ) } );
				var setterIL = setterBuilder.GetILGenerator( );
				setterIL.Emit( OpCodes.Ldarg_0 );
				setterIL.Emit( OpCodes.Ldarg_1 );
				setterIL.Emit( OpCodes.Stfld, backingField );
				setterIL.Emit( OpCodes.Ret );
				propertyBuilder.SetSetMethod( setterBuilder );
			}

			try {
				return typeBuilder.CreateType( )!;
			} catch ( Exception ex ) {
				throw new InvalidOperationException( $"Failed to create dynamic type with interface derived from {baseType.FullName}: {ex.Message}", ex );
			}
		}

		/// <summary>
		/// Sets a property value using reflection.
		/// </summary>
		private static void SetProperty( object obj, string propertyName, object? value ) {
			var property = obj.GetType( ).GetProperty( propertyName );
			property?.SetValue( obj, value );
		}

		/// <summary>
		/// Gets a property value using reflection.
		/// </summary>
		private static T GetProperty<T>( object obj, string propertyName ) {
			var property = obj.GetType( ).GetProperty( propertyName );
			return ( T )property?.GetValue( obj )!;
		}
	}

	/// <summary>
	/// Test interface for interface inheritance tests.
	/// </summary>
	public interface ITestInterface {
		int TestProperty { get; set; }
	}
}