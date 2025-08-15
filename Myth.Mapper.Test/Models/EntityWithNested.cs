using Myth.Interfaces;

namespace Myth.Mapper.Test.Models {

	public class EntityWithNested : IMapTo<DtoWithNested> {
		public int Id { get; set; }
		public List<NestedItem> Items { get; set; } = new( );

		public void MapTo( MappingBuilder<DtoWithNested> builder ) {
		}
	}
}