using Myth.Interfaces;
using Myth.Morph;

namespace Myth.Morph.Test.Models {

	public class DerivedEntity : BaseEntity, IMorphTo<DerivedDto> {
		public string DerivedProperty { get; set; } = "";

		public void Binder( BinderBuilder<DerivedDto> builder ) {
		}
	}
}