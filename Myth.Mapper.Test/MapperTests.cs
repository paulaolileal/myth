using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Extensions;
using Myth.Interfaces.Repositories.Results;
using Myth.Mapper.Test.Models;
using Myth.Mapper.Test.Service;
using Myth.Repositories.Results;

namespace Myth.Mapper.Test {

	public class MapperTests {
		private readonly IServiceCollection _services;
		private IServiceProvider _serviceProvider;

		public MapperTests( ) {
			_services = new ServiceCollection( );
			_services.AddSingleton<IDescriptionResolver, DescriptionResolver>( );
			_services.AddMapper( config => {
				config.AddGenericMapping( typeof( IPaginated<> ), typeof( Paginated<> ) );
			} );
			_serviceProvider = _services.BuildServiceProvider( );
		}

		[Fact]
		public void AddMapper_Should_RegisterMapRegistry( ) {
			// Act
			var registry = _serviceProvider.GetService<MapRegistry>( );

			// Assert
			registry.Should( ).NotBeNull( );
		}

		[Fact]
		public void MapRegistry_Should_RegisterGenericMapping( ) {
			// Arrange
			var registry = new MapRegistry( _serviceProvider );

			// Act
			registry.RegisterGenericMapping( typeof( IList<> ), typeof( List<> ) );

			// Assert
			var hasMapping = registry.TryResolveGenericConcrete( typeof( IList<string> ), out var concrete );
			hasMapping.Should( ).BeTrue( );
			concrete.Should( ).Be( typeof( List<string> ) );
		}

		[Fact]
		public void MapTo_Should_HandleNullValues( ) {
			// Arrange
			BasicEntity? entity = null;

			// Act
			var result = entity.MapTo<BasicDto>( );

			// Assert
			result.Should( ).BeNull( );
		}

		[Fact]
		public void MapTo_Should_HandleCollections( ) {
			// Arrange
			var entities = new List<BasicEntity>( );
			BasicEntity? entity = new BasicEntity {
				Description = "Test",
				Enabled = true,
				EntityId = 1,
				Name = "EntityA"
			};

			BasicEntity? entity2 = new BasicEntity {
				Description = "Test",
				Enabled = true,
				EntityId = 1,
				Name = "EntityA"
			};

			entities.Add( entity );
			entities.Add( entity2 );

			// Act
			var result = entities.MapTo<BasicDto>( );

			// Assert
			result.Should( ).NotBeNull( );
		}

		[Fact]
		public void MapTo_Should_HandleGenerics( ) {
			// Arrange
			var entities = new List<BasicEntity>( );
			BasicEntity? entity = new BasicEntity {
				Description = "Test",
				Enabled = true,
				EntityId = 1,
				Name = "EntityA"
			};

			BasicEntity? entity2 = new BasicEntity {
				Description = "Test",
				Enabled = true,
				EntityId = 1,
				Name = "EntityA"
			};

			entities.Add( entity );
			entities.Add( entity2 );

			// Act
			// Act
			var paginatedEntities = entities.AsPaginated( );
			var result = paginatedEntities.MapTo<IPaginated<BasicDto>>( _serviceProvider );

			// Assert
			result.Should( ).NotBeNull( );
			result.Items.Should( ).HaveCount( 2 );
			result.PageNumber.Should( ).Be( 1 );
			result.PageSize.Should( ).Be( 2 );
			result.TotalItems.Should( ).Be( 2 );
			result.TotalPages.Should( ).Be( 1 );

			// Verifica se os itens foram mapeados corretamente
			var firstItem = result.Items.First( );
			firstItem.Should( ).NotBeNull( );
			firstItem.DtoId.Should( ).Be( entity.EntityId );
			firstItem.Name.Should( ).Be( entity.Name );
			firstItem.Description.Should( ).Be( entity.Description );
			firstItem.Enabled.Should( ).Be( !entity.Enabled );

			var secondItem = result.Items.Skip( 1 ).First( );
			secondItem.Should( ).NotBeNull( );
			secondItem.DtoId.Should( ).Be( entity2.EntityId );
			secondItem.Name.Should( ).Be( entity2.Name );
			secondItem.Description.Should( ).Be( entity2.Description );
			secondItem.Enabled.Should( ).Be( !entity2.Enabled);
		}

		[Fact]
		public void MapTo_Should_MapNestedCollections( ) {
			// Arrange
			var entity = new EntityWithNested {
				Id = 1,
				Items = new List<NestedItem> {
					new() { Id = 1, Value = "One" },
					new() { Id = 2, Value = "Two" }
				}
			};

			// Act
			var dto = entity.MapTo<DtoWithNested>( _serviceProvider );

			// Assert
			dto.Items.Should( ).HaveCount( 2 );
			dto.Items.Should( ).BeEquivalentTo( entity.Items, opts =>
				opts.ComparingByMembers<NestedItem>( ) );
		}

		[Fact]
		public void MapTo_Should_HandleInheritance( ) {
			// Arrange
			var derived = new DerivedEntity {
				BaseProperty = "Base",
				DerivedProperty = "Derived"
			};

			// Act
			var dto = derived.MapTo<DerivedDto>( );

			// Assert
			dto.BaseProperty.Should( ).Be( "Base" );
			dto.DerivedProperty.Should( ).Be( "Derived" );
		}

		[Fact]
		public void MappingBuilder_Should_IgnoreSpecifiedProperties( ) {
			// Arrange
			var source = new SourceEntity {
				Id = 1,
				Name = "Test",
				IgnoredValue = "Should not map"
			};

			// Act
			var result = source.MapTo<DestEntity>( );

			// Assert
			result.Id.Should( ).Be( 1 );
			result.Name.Should( ).Be( "Test" );
			result.IgnoredProperty.Should( ).BeNull( );
		}

		[Fact]
		public void MapTo_Should_HandleAsyncMappings( ) {
			// Arrange
			var entity = new EntityWithAsync {
				Id = 1,
				AsyncValue = Task.FromResult( "Async Result" )
			};

			// Act
			var dto = entity.MapToAsync<DtoWithAsync>( _serviceProvider ).Result;

			// Assert
			dto.Id.Should( ).Be( 1 );
			dto.Value.Should( ).Be( "Async Result" );
		}

		[Fact]
		public void CanMapTo_Should_ReturnCorrectResult( ) {
			// Arrange
			var entity = new BasicEntity { EntityId = 1 };

			// Act
			var canMap = entity.CanMapTo<BasicDto>( _serviceProvider );
			var cannotMap = entity.CanMapTo<NonMappableDto>( _serviceProvider );

			// Assert
			canMap.Should( ).BeTrue( );
			cannotMap.Should( ).BeFalse( );
		}

		[Fact]
		public void MapTo_Should_HandleCircularReferences( ) {
			// Arrange
			var parent = new ParentEntity { Id = 1 };
			var child = new ChildEntity { Id = 2, Parent = parent };
			parent.Child = child;

			// Act
			var dto = parent.MapTo<ParentDto>( _serviceProvider );

			// Assert
			dto.Id.Should( ).Be( 1 );
			dto.Child.Should( ).NotBeNull( );
			dto.Child.Id.Should( ).Be( 2 );
			dto.Child.ParentId.Should( ).Be( 1 );
		}
	}
}

/*
 * Teste com entidades filhas como propriedades OK
 * Teste com entidades filhas como fields OK
 * Teste com entidades filhas complexas OK
 * Teste com injeção de dependências OK
 * Teste com entidades propriedades de listas e fields OK
 * Teste com enums OK
 */