using Myth.Extensions;
using Myth.Interfaces;
using Myth.Interfaces.Repositories.Results;

namespace Myth.Mapper.Test.Models {

	internal class BasicEntity : IMapTo<BasicEntity, BasicDto>, IMapTo<IPaginated<BasicEntity>, IPaginated<BasicDto>> {
		public int EntityId { get; set; }
		public string Name { get; set; }
		public bool Enabled { get; set; }
		public string Description { get; set; }

		public void MapTo( MappingBuilder<BasicEntity, BasicDto> builder ) {
			builder
				.ForMember(
					dest => dest.Enabled,
					( src, sp ) => !src.Enabled )
				.ForMember(
					dest => dest.DtoId,
					( src, sp ) => src.EntityId );
		}

		public void MapTo( MappingBuilder<IPaginated<BasicEntity>, IPaginated<BasicDto>> builder ) {
			builder
				.ForMember(
					dest => dest.Items,
					src => src.Items.MapTo<BasicDto>( ) );
		}
	}
}