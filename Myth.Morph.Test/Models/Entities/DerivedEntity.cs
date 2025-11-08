using Myth.Interfaces;
using Myth.Morph.Test.Models.Dtos;

namespace Myth.Morph.Test.Models {

	public class DerivedEntity : BaseEntity, IMorphableTo<DerivedDto> {
		public string DerivedProperty { get; set; } = "";

		public void MorphTo( Schema<DerivedDto> schema ) {
		}
	}
}