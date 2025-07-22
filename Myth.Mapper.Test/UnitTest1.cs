using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Extensions;
using System.Security.Cryptography.X509Certificates;

namespace Myth.Mapper.Test {
	public class UnitTest1 {
		[Fact]
		public void Mapper_should_map_origin_to_destination_with_map_to_respecting_rules_for_single_entity( ) {
			// Arrange
			var serviceCollection = new ServiceCollection( );
			serviceCollection.AddMapper( );

			var origin = new OriginForMapTo {
				OriginId = 1,
				Name = "Test Origin",
				Enabled = true
			};

			// Act
			var dest = origin.MapTo<DestinationForMapTo>( );

			// Assert	
			dest.DestinationId.Should( ).Be( origin.OriginId );
			dest.Name.Should( ).Be( origin.Name );
			dest.Enabled.Should( ).Be( !origin.Enabled );
		}

		[Fact]
		public void Mapper_should_map_origin_to_destination_with_map_to_respecting_rules_for_lists( ) {
			// Arrange
			var serviceCollection = new ServiceCollection( );
			serviceCollection.AddMapper( );

			var origin1 = new OriginForMapTo {
				OriginId = 1,
				Name = "Test Origin1",
				Enabled = true
			};

			var origin2 = new OriginForMapTo {
				OriginId = 2,
				Name = "Test Origin2",
				Enabled = false
			};

			var originList = new List<OriginForMapTo> { origin1, origin2 };

			// Act
			var dest = originList.MapTo<DestinationForMapTo>( );

			// Assert	
			dest.Should( ).NotBeNull( );
			dest.Should( ).HaveCount( 2 );

			var dest1 = dest.First();

			dest1.DestinationId.Should( ).Be( origin1.OriginId );
			dest1.Name.Should( ).Be( origin1.Name );
			dest1.Enabled.Should( ).Be( !origin1.Enabled );

			var dest2 = dest.Last( );

			dest2.DestinationId.Should( ).Be( origin2.OriginId );
			dest2.Name.Should( ).Be( origin2.Name );
			dest2.Enabled.Should( ).Be( !origin2.Enabled );
		}

		[Fact]
		public void Mapper_should_map_origin_to_destination_with_map_from_respecting_rules_for_single_entity( ) {
			// Arrange
			var serviceCollection = new ServiceCollection( );
			serviceCollection.AddMapper( );

			var origin = new OriginForMapFrom {
				OriginId = 1,
				Name = "Test Origin",
				Enabled = true
			};

			// Act
			var dest = origin.MapTo<DestinationForMapFrom>( );

			// Assert	
			dest.DestinationId.Should( ).Be( origin.OriginId );
			dest.Name.Should( ).Be( origin.Name );
			dest.Enabled.Should( ).Be( !origin.Enabled );
		}
	}
}