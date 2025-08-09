using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Extensions;
using Myth.Interfaces.Repositories.Results;
using Myth.Mapper.Test.Models;
using Myth.Mapper.Test.Service;
using Myth.Repositories.Results;

namespace Myth.Mapper.Test {

	public class MapperTests {
		private readonly ServiceCollection _serviceCollection;

		public MapperTests( ) {
			_serviceCollection = new ServiceCollection( );
			_serviceCollection.AddSingleton<IDescriptionResolver, DescriptionResolver>( );
			_serviceCollection.AddMapper( x => x.AddGenericMapping( typeof( IPaginated<> ), typeof( Paginated<> ) ) );
		}

		[Fact]
		public void Mapper_should_map_simple_properties( ) {
			// Arrange
			var entity = new EntityA {
				EntityId = 1,
				Name = "Test Origin",
				Enabled = true,
				Description = "This is a test entity",
				ItemsProp = [
					new( ) { ItemId = 1, Name = "Item 1" },
					new( ) { ItemId = 2, Name = "Item 2" }
				]
			};

			// Act
			var dto = entity.MapTo<Dto>( );

			// Assert
			dto.DtoId.Should( ).Be( entity.EntityId );
			dto.Name.Should( ).Be( entity.Name );
			dto.Description.Should( ).Be( entity.Description );
			dto.Enabled.Should( ).Be( !entity.Enabled );
			dto.ItemsProp.Should( ).BeEquivalentTo( entity.ItemsProp );
		}

		[Fact]
		public void Mapper_should_map_fields( ) {
			// Arrange
			var entity = new EntityB {
				EntityId = 1,
				Name = "Test Origin",
				Enabled = true,
				Description = "This is a test entity",
				ItemsField = [
					new( ) { ItemId = 1, Name = "Item 1" },
					new( ) { ItemId = 2, Name = "Item 2" }
				],
				ItemsProp = [
					new( ) { ItemId = 1, Name = "Item 1" },
					new( ) { ItemId = 2, Name = "Item 2" }
				]
			};

			// Act
			var dto = entity.MapTo<Dto>( );

			// Assert
			dto.DtoId.Should( ).Be( entity.EntityId );
			dto.Name.Should( ).Be( entity.Name );
			dto.Description.Should( ).Be( entity.Description );
			dto.Enabled.Should( ).Be( !entity.Enabled );
			dto.ItemsProp.Should( ).BeEquivalentTo( entity.ItemsProp );
			dto.ItemsField.Should( ).BeEquivalentTo( entity.ItemsField );
		}

		[Fact]
		public void Mapper_should_map_enum( ) {
			// Arrange
			var entity = new EntityC {
				EntityId = 1,
				Name = "Test Origin",
				Enabled = true,
				Description = "This is a test entity",
				Test = TestEnum.Two,
				ItemsField = [
					new( ) { ItemId = 1, Name = "Item 1" },
					new( ) { ItemId = 2, Name = "Item 2" }
				],
				ItemsProp = [
					new( ) { ItemId = 1, Name = "Item 1" },
					new( ) { ItemId = 2, Name = "Item 2" }
				]
			};

			// Act
			var dto = entity.MapTo<Dto>( );

			// Assert
			dto.DtoId.Should( ).Be( entity.EntityId );
			dto.Name.Should( ).Be( entity.Name );
			dto.Enabled.Should( ).Be( !entity.Enabled );
			dto.TestId.Should( ).Be( ( int )entity.Test );
			dto.TestDescription.Should( ).Be( entity.Test.ToString( ) );
		}

		[Fact]
		public void Mapper_ignore_should_ignore_property( ) {
			// Arrange
			var entity = new EntityD {
				EntityId = 1,
				Name = "Test Origin",
				Enabled = true,
				Description = "This is a test entity",
				Test = TestEnum.Two,
				ItemsField = [
					new( ) { ItemId = 1, Name = "Item 1" },
					new( ) { ItemId = 2, Name = "Item 2" }
				],
				ItemsProp = [
					new( ) { ItemId = 1, Name = "Item 1" },
					new( ) { ItemId = 2, Name = "Item 2" }
				]
			};

			// Act
			var dto = entity.MapTo<Dto>( );

			// Assert
			dto.DtoId.Should( ).Be( entity.EntityId );
			dto.Name.Should( ).Be( entity.Name );
			dto.Enabled.Should( ).Be( !entity.Enabled );
			dto.TestId.Should( ).Be( ( int )entity.Test );
			dto.TestDescription.Should( ).Be( entity.Test.ToString( ) );
			dto.Description.Should( ).Be( "No description" );
		}

		[Fact]
		public void Mapper_should_inject_service_for_resolve_property( ) {
			// Arrange

			var entity = new EntityE {
				EntityId = 1,
				Name = "Test Origin",
				Enabled = true,
				Description = "This is a test entity",
				Test = TestEnum.Two,
				ItemsField = [
					new( ) { ItemId = 1, Name = "Item 1" },
					new( ) { ItemId = 2, Name = "Item 2" }
				],
				ItemsProp = [
					new( ) { ItemId = 1, Name = "Item 1" },
					new( ) { ItemId = 2, Name = "Item 2" }
				]
			};

			// Act
			var dto = entity.MapTo<Dto>( );

			// Assert
			dto.DtoId.Should( ).Be( entity.EntityId );
			dto.Name.Should( ).Be( entity.Name );
			dto.Enabled.Should( ).Be( !entity.Enabled );
			dto.TestId.Should( ).Be( ( int )entity.Test );
			dto.TestDescription.Should( ).Be( entity.Test.ToString( ) );
			dto.Description.Should( ).Be( entity.Description.ToUpper( ) );
		}

		[Fact]
		public void Mapper_should_map_origin_to_destination_respecting_rules_for_lists( ) {
			// Arrange
			var serviceCollection = new ServiceCollection( );
			serviceCollection.AddMapper( );

			var origin1 = new EntityA {
				EntityId = 1,
				Name = "Test Origin1",
				Enabled = true
			};

			var origin2 = new EntityA {
				EntityId = 2,
				Name = "Test Origin2",
				Enabled = false
			};

			var originList = new List<EntityA> { origin1, origin2 };

			// Act
			var dest = originList.MapTo<Dto>( );

			// Assert
			dest.Should( ).NotBeNull( );
			dest.Should( ).HaveCount( 2 );

			var dest1 = dest.First( );

			dest1.DtoId.Should( ).Be( origin1.EntityId );
			dest1.Name.Should( ).Be( origin1.Name );
			dest1.Enabled.Should( ).Be( !origin1.Enabled );

			var dest2 = dest.Last( );

			dest2.DtoId.Should( ).Be( origin2.EntityId );
			dest2.Name.Should( ).Be( origin2.Name );
			dest2.Enabled.Should( ).Be( !origin2.Enabled );
		}

		[Fact]
		public void Mapper_should_map_complexes_properties( ) {
			// Arrange

			var entity = new EntityF {
				EntityId = 1,
				Name = "Test Origin",
				Item = new( ) { ItemId = 2, Name = "Item 2" }
			};

			// Act
			var dto = entity.MapTo<Dto>( );

			// Assert
			dto.DtoId.Should( ).Be( entity.EntityId );
			dto.Name.Should( ).Be( entity.Name );
			dto.Item.Should( ).BeEquivalentTo( entity.Item );
		}

		[Fact]
		public void Mapper_should_map_generic_interface( ) {
			// Arrange

			var origin1 = new EntityA {
				EntityId = 1,
				Name = "Test Origin1",
				Enabled = true
			};

			var origin2 = new EntityA {
				EntityId = 2,
				Name = "Test Origin2",
				Enabled = false
			};

			var originList = new List<EntityA> { origin1, origin2 };

			var paginatedEntities = new Paginated<EntityA>(
				0,
				originList.Count,
				originList.Count,
				1, originList );

			// Act
			var result = paginatedEntities.MapTo<Paginated<Dto>>( );
			// Assert
			result.Should( ).NotBeNull( );
			result.Items.Should( ).HaveCount( 2 );
			result.Items.First( ).DtoId.Should( ).Be( 1 );
			result.Items.Last( ).DtoId.Should( ).Be( 2 );
			result.TotalItems.Should( ).Be( 2 );
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