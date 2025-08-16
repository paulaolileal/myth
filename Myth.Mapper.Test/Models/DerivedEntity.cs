using Myth.Interfaces;

namespace Myth.Morph.Test.Models {

	public class DerivedEntity : BaseEntity, IMorphable<DerivedDto> {
		public string DerivedProperty { get; set; } = "";

		public void MorphTo( Schema<DerivedDto> builder ) {
		}
	}
}