using Myth.Interfaces;
using Myth.Morph.Test.Models.Dtos;

namespace Myth.Morph.Test.Models {

	internal class BasicEntity : IMorphable<BasicDto> {
		public int EntityId { get; set; }
		public string Name { get; set; }
		public bool Enabled { get; set; }
		public string Description { get; set; }

		public void MorphTo( Schema<BasicDto> schema ) {
			schema
				.Bind(
					dest => dest.Enabled,
					( ) => !Enabled )
				.Bind(
					dest => dest.DtoId,
					( ) => EntityId );
		}
	}
}