using Myth.Interfaces;

namespace Myth.Mapper.Test.Models {

	internal class BasicDto : IMapTo<BasicEntity>, IMapTo<ViewModel> {
		public int DtoId { get; set; }
		public string Name { get; set; }
		public bool Enabled { get; set; }
		public int TestId { get; set; }
		public string TestDescription { get; set; }
		public DtoItem Item { get; set; }
		public IEnumerable<DtoItem> ItemsField = [ ];
		public IEnumerable<DtoItem> ItemsProp { get; set; } = [ ];
		public string Description { get; set; } = "No description";

		public void MapTo( MappingBuilder<BasicEntity> builder ) {
			builder
				.ForMember(
					dest => dest.Enabled,
					( ) => !Enabled )
				.ForMember(
					dest => dest.EntityId,
					( ) => DtoId );
		}

		public void MapTo( MappingBuilder<ViewModel> builder ) {
			builder
				.ForMember(
					dest => dest.ViewModelId,
					( ) => DtoId );
		}
	}

	internal class DtoItem {
		public int ItemId { get; set; }
		public string Name { get; set; }
	}
}