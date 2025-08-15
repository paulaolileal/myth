using Myth.Interfaces;

namespace Myth.Mapper.Test.Models {

	internal class BasicEntity : IMapTo<BasicDto> {
		public int EntityId { get; set; }
		public string Name { get; set; }
		public bool Enabled { get; set; }
		public string Description { get; set; }

		public void MapTo( MappingBuilder<BasicDto> builder ) {
			builder
				.ForMember(
					dest => dest.Enabled,
					( ) => !Enabled )
				.ForMember(
					dest => dest.DtoId,
					( ) => EntityId );
		}
	}
}