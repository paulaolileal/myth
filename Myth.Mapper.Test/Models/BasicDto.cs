using Myth.Interfaces;

namespace Myth.Mapper.Test.Models {

	internal class BasicDto : IMapTo<BasicDto, BasicEntity>, IMapTo<BasicDto, ViewModel> {
		public int DtoId { get; set; }
		public string Name { get; set; }
		public bool Enabled { get; set; }
		public int TestId { get; set; }
		public string TestDescription { get; set; }
		public DtoItem Item { get; set; }
		public IEnumerable<DtoItem> ItemsField = [ ];
		public IEnumerable<DtoItem> ItemsProp { get; set; } = [ ];
		public string Description { get; set; } = "No description";

		public void MapTo( MappingBuilder<BasicDto, BasicEntity> builder ) {
			builder
				.ForMember(
					dest => dest.Enabled,
					( src, sp ) => !src.Enabled )
				.ForMember(
					dest => dest.EntityId,
					( src, sp ) => src.DtoId );
		}

		public void MapTo( MappingBuilder<BasicDto, ViewModel> builder ) {
			builder
				.ForMember(
					dest => dest.ViewModelId,
					( src, sp ) => src.DtoId );
		}
	}

	internal class DtoItem {
		public int ItemId { get; set; }
		public string Name { get; set; }
	}
}