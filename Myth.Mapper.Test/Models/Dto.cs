using Myth.Interfaces;

namespace Myth.Mapper.Test.Models {

	internal class Dto : IMapTo<Dto, Entity>, IMapTo<Dto, ViewModel> {
		public int DtoId { get; set; }
		public string Name { get; set; }
		public bool Enabled { get; set; }
		public string Description { get; set; } = "No description";

		public void MapTo( MappingBuilder<Dto, Entity> builder ) {
			builder
				.ForMember(
					dest => dest.Enabled,
					( src, sp ) => !src.Enabled )
				.ForMember(
					dest => dest.EntityId,
					( src, sp ) => src.DtoId );
		}

		public void MapTo( MappingBuilder<Dto, ViewModel> builder ) {
			builder
				.ForMember(
					dest => dest.ViewModelId,
					( src, sp ) => src.DtoId );
		}
	}
}