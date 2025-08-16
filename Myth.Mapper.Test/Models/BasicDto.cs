using Myth.Interfaces;

namespace Myth.Morph.Test.Models {

	internal class BasicDto : IMorphable<BasicEntity>, IMorphable<ViewModel> {
		public int DtoId { get; set; }
		public string Name { get; set; }
		public bool Enabled { get; set; }
		public int TestId { get; set; }
		public string TestDescription { get; set; }
		public DtoItem Item { get; set; }
		public IEnumerable<DtoItem> ItemsField = [ ];
		public IEnumerable<DtoItem> ItemsProp { get; set; } = [ ];
		public string Description { get; set; } = "No description";

		public void MorphTo( Schema<BasicEntity> builder ) {
			builder
				.Bind(
					dest => dest.Enabled,
					( ) => !Enabled )
				.Bind(
					dest => dest.EntityId,
					( ) => DtoId );
		}

		public void MorphTo( Schema<ViewModel> builder ) {
			builder
				.Bind(
					dest => dest.ViewModelId,
					( ) => DtoId );
		}
	}
}