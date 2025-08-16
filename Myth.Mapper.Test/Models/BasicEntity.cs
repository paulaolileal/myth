using Myth.Interfaces;

namespace Myth.Morph.Test.Models {

	internal class BasicEntity : IMorphTo<BasicDto> {
		public int EntityId { get; set; }
		public string Name { get; set; }
		public bool Enabled { get; set; }
		public string Description { get; set; }

		public void Binder( BinderBuilder<BasicDto> builder ) {
			builder
				.Bind(
					dest => dest.Enabled,
					( ) => !Enabled )
				.Bind(
					dest => dest.DtoId,
					( ) => EntityId );
		}
	}
}