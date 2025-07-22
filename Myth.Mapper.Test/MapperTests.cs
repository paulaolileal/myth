using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Extensions;
using Myth.Mapper.Test.Models;

namespace Myth.Mapper.Test {

	public class MapperTests {

		[Fact]
		public void Mapper_should_map_origin_to_destination_respecting_rules_for_single_entity( ) {
			// Arrange
			var serviceCollection = new ServiceCollection( );
			serviceCollection.AddMapper( );

			var entity = new Entity {
				EntityId = 1,
				Name = "Test Origin",
				Enabled = true,
				Description = "This is a test entity"
			};

			// Act
			var dto = entity.MapTo<Dto>( );

			var viewModel = dto.MapTo<ViewModel>( );

			// Assert
			dto.DtoId.Should( ).Be( entity.EntityId );
			dto.Name.Should( ).Be( entity.Name );
			dto.Description.Should( ).Be( "No description" );
			dto.Enabled.Should( ).Be( !entity.Enabled );

			viewModel.ViewModelId.Should( ).Be( dto.DtoId );
			viewModel.Name.Should( ).Be( dto.Name );
			viewModel.Enabled.Should( ).Be( dto.Enabled );
		}

		[Fact]
		public void Mapper_should_map_origin_to_destination_respecting_rules_for_lists( ) {
			// Arrange
			var serviceCollection = new ServiceCollection( );
			serviceCollection.AddMapper( );

			var origin1 = new Entity {
				EntityId = 1,
				Name = "Test Origin1",
				Enabled = true
			};

			var origin2 = new Entity {
				EntityId = 2,
				Name = "Test Origin2",
				Enabled = false
			};

			var originList = new List<Entity> { origin1, origin2 };

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
	}
}