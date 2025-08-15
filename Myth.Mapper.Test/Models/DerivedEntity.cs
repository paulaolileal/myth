using Myth.Interfaces;

namespace Myth.Mapper.Test.Models {

	public class DerivedEntity : BaseEntity, IMapTo<DerivedDto> {
		public string DerivedProperty { get; set; } = "";

		public void MapTo( MappingBuilder<DerivedDto> builder ) {
		}
	}
}