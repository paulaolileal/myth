using Myth.Interfaces;

namespace Myth.Mapper.Test.Models {

	internal class Entity : IMapTo<Entity, Dto> {
		public int EntityId { get; set; }
		public string Name { get; set; }
		public bool Enabled { get; set; }
		public string Description { get; set; }

		public void MapTo( MappingBuilder<Entity, Dto> builder ) {
			builder
				.ForMember(
					dest => dest.Enabled,
					( src, sp ) => !src.Enabled )
				.ForMember(
					dest => dest.DtoId,
					( src, sp ) => src.EntityId )
				.Ignore( dest => dest.Description );
		}
	}
}